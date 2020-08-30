namespace TeisterMask.DataProcessor
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Data;
    using Microsoft.SqlServer.Server;
    using Newtonsoft.Json;
    using TeisterMask.DataProcessor.ExportDto;
    using Formatting = Newtonsoft.Json.Formatting;

    public class Serializer
    {
        public static string ExportMostBusiestEmployees(TeisterMaskContext context, DateTime date)
        {
            var employees = context.Employees
                .AsEnumerable()
                .Where(e => e.EmployeesTasks.Any(et => et.Task.OpenDate >= date))
                .OrderByDescending(x => x.EmployeesTasks.Where(et => et.Task.OpenDate >= date).Count())
                .ThenBy(x => x.Username)
                .Take(10)
                .Select(e => new ExportEmployeeDto()
                {
                    Username = e.Username,
                    Tasks = e.EmployeesTasks
                    .Where(et => et.Task.OpenDate >= date)
                    .OrderByDescending(x => x.Task.DueDate)
                    .ThenBy(x => x.Task.Name)
                    .Select(et => new ExportEmployeeTaskDto()
                    {
                        TaskName = et.Task.Name,
                        OpenDate = et.Task.OpenDate.ToString("d", CultureInfo.InvariantCulture),
                        DueDate = et.Task.DueDate.ToString("d", CultureInfo.InvariantCulture),
                        ExecutionType = et.Task.ExecutionType.ToString(),
                        LabelType = et.Task.LabelType.ToString(),
                    })
                    .ToArray()
                })
                .ToArray();

            var json = JsonConvert.SerializeObject(employees, Formatting.Indented);

            return json;
        }

        public static string ExportProjectWithTheirTasks(TeisterMaskContext context)
        {
            var porjects = context.Projects
                .AsEnumerable()
                .Where(p => p.Tasks.Any())
                .Select(p => new ExportProjectDto()
                {
                    TaskCount = p.Tasks.Count(),
                    ProjectName = p.Name,
                    HasEndDate = p.DueDate == null ? "No" : "Yes",
                    Tasks = p.Tasks
                    .OrderBy(x=>x.Name)
                    .Select(t=> new ExportProjectTaskDto()
                    { 
                        Name= t.Name,
                        Label = t.LabelType.ToString()
                    })
                    .ToArray()
                })
                .OrderByDescending(x=>x.TaskCount)
                .ThenBy(x=>x.ProjectName)
                .ToArray();

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, string.Empty);

            var xmlSerializer = new XmlSerializer(typeof(ExportProjectDto[]), new XmlRootAttribute("Projects"));

            var sb = new StringBuilder();
            var stream = new StringWriter(sb);
            xmlSerializer.Serialize(stream, porjects, ns);

            return sb.ToString().TrimEnd();
        }
    }
}