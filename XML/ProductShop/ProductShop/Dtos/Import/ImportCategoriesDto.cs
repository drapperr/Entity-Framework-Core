using ProductShop.Models;
using System.Xml.Serialization;

namespace ProductShop.Dtos.Import
{
    [XmlType(nameof(Category))]
    public class ImportCategoriesDto
    {
        [XmlElement("name")]
        public string Name { get; set; }
    }
}
