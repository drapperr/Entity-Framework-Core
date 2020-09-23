namespace SoftJail.DataProcessor
{

    using Data;
    using Newtonsoft.Json;
    using SoftJail.DataProcessor.ExportDto;
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;

    public class Serializer
    {
        public static string ExportPrisonersByCells(SoftJailDbContext context, int[] ids)
        {
            var prisoners = context.Prisoners
                .Where(p => ids.Contains(p.Id))
                .Select(p => new
                {
                    Id = p.Id,
                    Name = p.FullName,
                    CellNumber = p.Cell.CellNumber,
                    Officers = p.PrisonerOfficers
                    .Select(po => new
                    {
                        OfficerName = po.Officer.FullName,
                        Department = po.Officer.Department.Name
                    })
                    .OrderBy(x=>x.OfficerName)
                    .ToArray(),
                    TotalOfficerSalary = decimal.Parse(p.PrisonerOfficers.Sum(po => po.Officer.Salary).ToString("F2"))
                })
                .OrderBy(x=>x.Name)
                .ThenBy(x=>x.Id)
                .ToArray();

            var jsonResult = JsonConvert.SerializeObject(prisoners, Formatting.Indented);

            return jsonResult;
        }

        public static string ExportPrisonersInbox(SoftJailDbContext context, string prisonersNames)
        {
            var prisonersNamesList = prisonersNames.Split(",").ToList();

            var prisoners = context.Prisoners
                .Where(p=>prisonersNamesList.Contains(p.FullName))
                .Select(p=> new ExportPrisonerDto
                {
                    Id = p.Id,
                    Name = p.FullName,
                    IncarcerationDate = p.IncarcerationDate.ToString("yyyy-MM-dd",CultureInfo.InvariantCulture),
                    EncryptedMessages = p.Mails
                    .Select(m=> new MessageDto
                    {
                        Description = Encrypt(m.Description)
                    })
                    .ToArray()
                })
                .OrderBy(x=>x.Name)
                .ThenBy(x=>x.Id)
                .ToArray();


            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, string.Empty);

            var xmlSerializer = new XmlSerializer(typeof(ExportPrisonerDto[]), new XmlRootAttribute("Prisoners"));

            var sb = new StringBuilder();

            xmlSerializer.Serialize(new StringWriter(sb), prisoners, ns);

            return sb.ToString().TrimEnd();
        }

        private static string Encrypt(string description)
        {
            char[] charArray = description.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
    }
}