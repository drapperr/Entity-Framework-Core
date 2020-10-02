using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace CarDealer.Dto.Imort
{
    [XmlType("Sale")]
    public class ImportSalesDto
    {
        [XmlElement("carId")]
        public int CarId { get; set; }

        [XmlElement("customerId")]
        public int CustomerId { get; set; }

        [XmlElement("discount")]
        public decimal Discount { get; set; }
    }
}

//<Sale>
//        <carId>342</carId>
//        <customerId>29</customerId>
//        <discount>0</discount>
//    </Sale>
