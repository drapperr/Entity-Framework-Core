using System.Xml.Serialization;

namespace CarDealer.Dto.Imort
{
    [XmlType("Part")]
    public class ImportCarPartDto
    {
        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("price")]
        public decimal Price { get; set; }

        [XmlElement("quantity")]
        public int Quantity { get; set; }

        [XmlElement("supplierId")]
        public int SupplierId { get; set; }
    }
}

//<name>Bonnet/hood</name>
//        <price>1001.34</price>
//        <quantity>10</quantity>
//        <supplierId>17</supplierId>
