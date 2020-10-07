using ProductShop.Models;
using System.Xml.Serialization;

namespace ProductShop.Dtos.Import
{
    [XmlType(nameof(User))]
    public class ImportUserDto
    {
        [XmlElement("firstName")]
        public string FirstName { get; set; }

        [XmlElement("lastName")]
        public string LastName { get; set; }

        [XmlElement("age")]
        public int Age { get; set; }
    }
}

 //<User>
 //       <firstName>Chrissy</firstName>
 //       <lastName>Falconbridge</lastName>
 //       <age>50</age>
 //   </User>
