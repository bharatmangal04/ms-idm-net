﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using System.Xml;
using IdmNet.SoapModels;

namespace IdmNet
{
    public class IdmNetClient
    {
        private readonly SearchClient _searchClient;
        private readonly ResourceFactoryClient _factoryClient;
        private readonly ResourceClient _resourceClient;

        public IdmNetClient(SearchClient searchClient, ResourceFactoryClient factoryClient, ResourceClient resourceClient)
        {
            _searchClient = searchClient;
            _factoryClient = factoryClient;
            _resourceClient = resourceClient;
        }

        public async Task<IEnumerable<IdmResource>> GetAsync(SearchCriteria criteria)
        {
            var pullInfo = await EnumerateAndPreparePull(criteria);

            // Pull all results
            PullResponse pullResponseObj;
            var results = new List<IdmResource>();
            EnumerationContext enumerationContext = pullInfo.EnumerationContext;
            do
            {
                pullResponseObj = await Pull(criteria.PageSize, enumerationContext, results);
            } while (pullResponseObj.EndOfSequence == null);

            Sort(results, criteria);

            return results;
        }

        private static void Sort(List<IdmResource> results, SearchCriteria criteria)
        {
            string attrName = criteria.SortAttribute;
            if (attrName == null)
                return;

            var negateIfNeeded = ((criteria.SortDecending) ? -1 : 1);

            results.Sort((res1, res2) => CompareResult(res1, res2, attrName, negateIfNeeded));
        }

        private static int CompareResult(IdmResource res1, IdmResource res2, string attrName, int negateIfNeeded)
        {
            var val1 = res1[attrName].Value ?? "";
            var val2 = res2[attrName].Value ?? "";
            var compareResult = String.Compare(val1.ToLower(), val2.ToLower(),
                StringComparison.Ordinal)*negateIfNeeded;
            return compareResult;
        }

        private async Task<PullInfo> EnumerateAndPreparePull(SearchCriteria criteria)
        {
            // Enumerate request
            var enumerateMessage = Message.CreateMessage(
                MessageVersion.Default,
                SoapConstants.EnumerateAction,
                new Enumerate
                {
                    Filter = new Filter(criteria.XPath),
                    Selection = criteria.Attributes,
                    Sorting = new Sorting { SortingAttribute = new SortingAttribute { AttributeName = criteria.SortAttribute, Ascending = !criteria.SortDecending } }
                },
                new SoapXmlSerializer(typeof(Enumerate)));
            Trace.WriteLine(enumerateMessage);
            var enumerateResponseMessage = await _searchClient.EnumerateAsync(enumerateMessage);


            // Check for enumerate fault
            if (enumerateResponseMessage.IsFault)
                throw new SoapFaultException("Enumerate Fault: " + enumerateResponseMessage);


            // Prepare first Pull
            if (criteria.PageSize == 0)
                criteria.PageSize = int.MaxValue;


            var pullInfo = new PullInfo
            {
                PageSize = criteria.PageSize,
                EnumerateResponse =
                    enumerateResponseMessage.GetBody<EnumerateResponse>(new SoapXmlSerializer(typeof(EnumerateResponse))),
            };
            return pullInfo;
        }

        private async Task<PullResponse> Pull(int pageSize, EnumerationContext enumerationContext, List<IdmResource> results)
        {
            var pullMessage = Message.CreateMessage(
                MessageVersion.Default,
                "http://schemas.xmlsoap.org/ws/2004/09/enumeration/Pull",
                new Pull
                {
                    EnumerationContext = enumerationContext,
                    MaxElements = pageSize,
                    PullAdjustment =
                        new PullAdjustment { StartingIndex = enumerationContext.CurrentIndex, EnumerationDirection = "Forwards" }
                },
                new SoapXmlSerializer(typeof(Pull)));

            var pullResponseMessage = await _searchClient.PullAsync(pullMessage);


            // Check for Pull fault
            if (pullResponseMessage.IsFault)
                throw new SoapFaultException("Pull Fault: " + pullResponseMessage);


            // Get Resources from Pull response
            var pullResponseObj = pullResponseMessage.GetBody<PullResponse>(new SoapXmlSerializer(typeof(PullResponse)));
            if (pullResponseObj.Items != null)
            {
                var xmlNodes = (XmlNode[])pullResponseObj.Items;
                results.AddRange(xmlNodes.Select(BuildResource));

                enumerationContext.CurrentIndex += xmlNodes.Length;
            }
            return pullResponseObj;
        }

        private static IdmResource BuildResource(XmlNode xmlNode)
        {
            var resource = new IdmResource();

            foreach (XmlNode attribute in xmlNode.ChildNodes)
                BuildAttribute(attribute, resource);

            return resource;
        }

        private static void BuildAttribute(XmlNode attribute, IdmResource resource)
        {
            string name = attribute.LocalName;
            string val = attribute.InnerText;

            if (val.StartsWith("urn:uuid:"))
                val = val.Substring(9);

            var attr = resource.GetAttr(name);
            if (attr != null)
                attr.Values.Add(val);
            else
                resource.Attributes.Add(new IdmAttribute {Name = name, Value = val});
        }


        public async Task<IdmResource> PostAsync(IdmResource resource)
        {
            if (resource == null)
                throw new ArgumentNullException("resource");

            var createRequestMessage = BuildCreateRequestMessage(resource);

            // Add the required header for the Create action
            createRequestMessage.Headers.Add(MessageHeader.CreateHeader("IdentityManagementOperation", SoapConstants.DirectoryAccess, null, true));


            Message addResponseMsg = await _factoryClient.CreateAsync(createRequestMessage);

            if (addResponseMsg.IsFault)
                throw new SoapFaultException("Create Fault: " + addResponseMsg);

            // Deserialize the Add response
            ResourceCreated resourceCreatedObject = addResponseMsg.GetBody<ResourceCreated>(new SoapXmlSerializer(typeof(ResourceCreated)));

            resource.ObjectID = resourceCreatedObject.EndpointReference.ReferenceProperties.ResourceReferenceProperty.Value;

            if (resource.ObjectID.StartsWith("urn:uuid:"))
                resource.ObjectID = resource.ObjectID.Substring(9);

            return resource;
        }

        private static Message BuildCreateRequestMessage(IdmResource resource)
        {
            var factoryRequest = BuildFactoryRequest(resource);

            var createRequestMessage = CreateSoapMessage(factoryRequest);

            return createRequestMessage;
        }

        private static Message CreateSoapMessage(AddRequest factoryRequest)
        {
            var createRequestMessage = Message.CreateMessage(MessageVersion.Default,
                SoapConstants.CreateAction,
                factoryRequest,
                new SoapXmlSerializer(typeof (AddRequest))
                );
            return createRequestMessage;
        }

        private static AddRequest BuildFactoryRequest(IdmResource resource)
        {
            var values = from attribute in resource.Attributes
                from val in attribute.Values
                select new AttributeTypeAndValue(attribute.Name, val);
            var factoryRequest = new AddRequest {AttributeTypeAndValue = values.ToArray()};
            return factoryRequest;
        }


        public async Task DeleteAsync(string objectID)
        {
            if (String.IsNullOrWhiteSpace(objectID))
                throw new ArgumentNullException("objectID");

            Message deleteRequestMsg = Message.CreateMessage(MessageVersion.Default, SoapConstants.DeleteAction);

            deleteRequestMsg.Headers.Add(MessageHeader.CreateHeader("ResourceReferenceProperty", SoapConstants.RmNamespace, objectID));

            Message deleteResponseMsg = await _resourceClient.DeleteAsync(deleteRequestMsg);
            if (deleteResponseMsg.IsFault)
                throw new SoapFaultException("Delete Fault: " + deleteResponseMsg);
        }

        public async Task AddValueAsync(string objectID, string attrName, string attrValue)
        {
            await BuildAndExecutePut(objectID, attrName, attrValue, ModeType.Add);
        }

        public async Task RemoveValueAsync(string objectID, string attrName, string attrValue)
        {
            await BuildAndExecutePut(objectID, attrName, attrValue, ModeType.Delete);
        }

        private async Task BuildAndExecutePut(string objectID, string attrName, string attrValue, ModeType modeType)
        {
            ModifyRequest modifyRequest = new ModifyRequest();
            Change changeRemoveAttribute = new Change(modeType, attrName, attrValue);
            modifyRequest.Change = new[] {changeRemoveAttribute};

            await Put(objectID, modifyRequest);
        }

        private async Task Put(string objectID, ModifyRequest modifyRequest)
        {
            // Create the Put request messsage
            Message putRequestMsg = Message.CreateMessage(MessageVersion.Default,
                SoapConstants.PutAction,
                modifyRequest,
                new SoapXmlSerializer(typeof (ModifyRequest))
                );

            // Add the ResourceReferenceProperty header for the Put request
            putRequestMsg.Headers.Add(MessageHeader.CreateHeader("ResourceReferenceProperty", SoapConstants.RmNamespace,
                objectID));
            putRequestMsg.Headers.Add(MessageHeader.CreateHeader("IdentityManagementOperation", SoapConstants.DirectoryAccess,
                null, true));

            Message putResponseMsg = await _resourceClient.PutAsync(putRequestMsg);

            if (putResponseMsg.IsFault)
                throw new SoapFaultException("Put Fault: " + putResponseMsg);
        }

        public async Task ReplaceValueAsync(string objectID, string attrName, string attrValue)
        {
            await BuildAndExecutePut(objectID, attrName, attrValue, ModeType.Replace);
        }
    }
}
