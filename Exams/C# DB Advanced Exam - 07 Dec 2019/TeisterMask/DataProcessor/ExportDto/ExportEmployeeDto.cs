namespace TeisterMask.DataProcessor.ExportDto
{
    public class ExportEmployeeDto
    {
        public string Username { get; set; }

        public ExportEmployeeTaskDto[] Tasks { get; set; }
    }
}
