using System;
using System.Xml.Serialization;

namespace CarDealer.Dto.Imort
{
    [XmlType("Customer")]
    public class ImportCustomerDto
    {
        [XmlElement("name")]
        public string name { get; set; }

        [XmlElement("birthDate")]
        public DateTime BirthDate { get; set; }

        [XmlElement("isYoungDriver")]
        public bool IsYanoungDriver { get; set; }
    }
}


//<Customer>
//        <name>Emmitt Benally</name>
//        <birthDate>1993-11-20T00:00:00</birthDate>
//        <isYoungDriver>true</isYoungDriver>
//    </Customer>