using System.ComponentModel.DataAnnotations;
using System.Xml;
using System.Xml.Serialization;

namespace TeisterMask.DataProcessor.ImportDto
{
    [XmlType("Task")]
    public class ImportProjectTaskDto
    {
        [XmlElement("Name")]
        [Required]
        [MinLength(2)]
        [MaxLength(40)]
        public string Name { get; set; }

        [XmlElement("OpenDate")]
        [Required]
        public string OpenDate { get; set; }

        [Required]
        [XmlElement("DueDate")]
        public string DueDate { get; set; }

        [XmlElement("ExecutionType")]
        [Required]
        [Range(0,3)]
        public int ExecutionType { get; set; }

        [XmlElement("LabelType")]
        [Required]
        [Range(0, 4)]
        public int LabelType { get; set; }
    }
}

// <Tasks>
//      <Task>
//        <Name>Australian</Name>
//        <OpenDate>19/08/2018</OpenDate>
//        <DueDate>13/07/2019</DueDate>
//        <ExecutionType>2</ExecutionType>
//        <LabelType>0</LabelType>
//      </Task>
