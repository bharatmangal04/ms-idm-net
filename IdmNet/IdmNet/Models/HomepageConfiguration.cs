using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
// ReSharper disable InconsistentNaming

namespace IdmNet.Models
{
    /// <summary>
    /// HomepageConfiguration - These resources drive the homepage appearance in FIM Portal.
    /// </summary>
    public class HomepageConfiguration : IdmResource
    {
        /// <summary>
        /// Parameterless CTOR
        /// </summary>
        public HomepageConfiguration()
        {
            ObjectType = ForcedObjType = "HomepageConfiguration";
        }

        /// <summary>
        /// Build a HomepageConfiguration object from a IdmResource object
        /// </summary>
        /// <param name="resource">base class</param>
        public HomepageConfiguration(IdmResource resource)
        {
            Attributes = resource.Attributes;
            ObjectType = ForcedObjType = "HomepageConfiguration";
            if (resource.Creator == null)
                return;
            Creator = resource.Creator;
        }

        readonly string ForcedObjType;

        /// <summary>
        /// Object Type (can only be HomepageConfiguration)
        /// </summary>
        [Required]
        public override sealed string ObjectType
        {
            get { return GetAttrValue("ObjectType"); }
            set
            {
                if (value != ForcedObjType)
                    throw new InvalidOperationException("Object Type of HomepageConfiguration can only be 'HomepageConfiguration'");
                SetAttrValue("ObjectType", value);
            }
        }

        /// <summary>
        /// Image Url - Image url for the a given element.
        /// </summary>
        public string ImageUrl
        {
            get { return GetAttrValue("ImageUrl"); }
            set {
                SetAttrValue("ImageUrl", value); 
            }
        }


        /// <summary>
        /// Is Configuration Type - This is an indication that this resource is a configuration resource.
        /// </summary>
        public bool? IsConfigurationType
        {
            get { return AttrToNullableBool("IsConfigurationType"); }
            set { 
                SetAttrValue("IsConfigurationType", value.ToString());
            }
        }


        /// <summary>
        /// Navigation Url - URL for navigation when user clicks this item.
        /// </summary>
        [Required]
        public string NavigationUrl
        {
            get { return GetAttrValue("NavigationUrl"); }
            set {
                SetAttrValue("NavigationUrl", value); 
            }
        }


        /// <summary>
        /// Order - Precedence of this item within a parent grouping
        /// </summary>
        [Required]
        public int Order
        {
            get { return AttrToInteger("Order"); }
            set { 
                SetAttrValue("Order", value.ToString());
            }
        }


        /// <summary>
        /// Parent Order - Parent grouping for this home page resource.
        /// </summary>
        [Required]
        public int ParentOrder
        {
            get { return AttrToInteger("ParentOrder"); }
            set { 
                SetAttrValue("ParentOrder", value.ToString());
            }
        }


        /// <summary>
        /// Region - Specifies where the item will be shown in the UI.
        /// </summary>
        [Required]
        public int Region
        {
            get { return AttrToInteger("Region"); }
            set { 
                SetAttrValue("Region", value.ToString());
            }
        }


        /// <summary>
        /// Resource Count - Count resources associated with this item (optional)
        /// </summary>
        public string CountXPath
        {
            get { return GetAttrValue("CountXPath"); }
            set {
                SetAttrValue("CountXPath", value); 
            }
        }


        /// <summary>
        /// Usage Keyword - 
        /// </summary>
        public List<string> UsageKeyword
        {
            get { return GetAttrValues("UsageKeyword"); }
            set {
                SetAttrValues("UsageKeyword", value); 
            }
        }


    }
}
