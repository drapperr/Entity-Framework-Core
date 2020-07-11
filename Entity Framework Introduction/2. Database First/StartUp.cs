using SoftUni.Data;
using SoftUni.Models;
using System;
using System.Linq;
using System.Text;

namespace SoftUni
{
    public class StartUp
    {
        static void Main(string[] args)
        {
            var context = new SoftUniContext();
            Console.WriteLine(IncreaseSalaries(context));
        }

        public static string GetEmployeesFullInformation(SoftUniContext context)
        {
            var employees = context.Employees
                .OrderBy(e => e.EmployeeId)
                .Select(e => $"{e.FirstName} {e.LastName} {e.MiddleName} {e.JobTitle} {e.Salary:F2}")
                .ToList();

            return string.Join(Environment.NewLine, employees);
        }

        public static string GetEmployeesWithSalaryOver50000(SoftUniContext context)
        {
            var filtredEmployees = context.Employees
                .Where(e => e.Salary > 50000)
                .OrderBy(e => e.FirstName)
                .Select(e => $"{e.FirstName} - {e.Salary:F2}")
                .ToList();

            return string.Join(Environment.NewLine, filtredEmployees);
        }

        public static string GetEmployeesFromResearchAndDevelopment(SoftUniContext context)
        {
            var filtredEmployees = context.Employees.Where(e => e.Department.Name == "Research and Development")
                .OrderBy(e => e.Salary)
                .ThenByDescending(e => e.FirstName)
                .Select(e => $"{e.FirstName} {e.LastName} from {e.Department.Name} - ${e.Salary:F2}")
                .ToList();

            return string.Join(Environment.NewLine, filtredEmployees);
        }

        public static string AddNewAddressToEmployee(SoftUniContext context)
        {
            var newAdress = new Address { AddressText = "Vitoshka 15", TownId = 4 };

            var employee = context.Employees.FirstOrDefault(e => e.LastName == "Nakov");

            employee.Address = newAdress;

            context.SaveChanges();

            var adresses = context.Employees
                .OrderByDescending(e => e.AddressId)
                .Take(10)
                .Select(e => e.Address.AddressText)
                .ToList();

            return string.Join(Environment.NewLine, adresses);
        }

        public static string GetEmployeesInPeriod(SoftUniContext context)
        {
            var sb = new StringBuilder();

            var emplyees = context.Employees
                .Where(e => e.EmployeesProjects
                            .Any(ep => ep.Project.StartDate.Year >= 2001 && ep.Project.StartDate.Year <= 2003))
                .Take(10)
                .Select(e => new
                {
                    e.FirstName,
                    e.LastName,
                    ManagerName = e.Manager.FirstName + " " + e.Manager.LastName,
                    Projects = e.EmployeesProjects
                    .Select(ep => new
                    {
                        ep.Project.Name,
                        ep.Project.StartDate,
                        ep.Project.EndDate,
                    }),
                });

            foreach (var employee in emplyees)
            {
                sb.AppendLine($"{employee.FirstName} {employee.LastName} - Manager: {employee.ManagerName}");

                foreach (var project in employee.Projects)
                {
                    var endDate = project.EndDate == null ? "not finished" : project.EndDate.ToString();
                    sb.AppendLine($"--{project.Name} - {project.StartDate} - {endDate}");
                }
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetAddressesByTown(SoftUniContext context)
        {
            var adresses = context.Addresses
                .Select(a => new
                {
                    a.AddressText,
                    TownName = a.Town.Name,
                    EmployeeCount = a.Employees.Count()
                })
                .OrderByDescending(a => a.EmployeeCount)
                .ThenBy(a => a.TownName)
                .ThenBy(a => a.AddressText)
                .Take(10)
                .Select(a => $"{a.AddressText}, {a.TownName} - {a.EmployeeCount} employees");

            return string.Join(Environment.NewLine,adresses);
        }

        public static string GetEmployee147(SoftUniContext context)
        {
            var employee147 = context.Employees
                .Where(e => e.EmployeeId == 147)
                .Select(e => new
                {
                    e.FirstName,
                    e.LastName,
                    e.JobTitle,
                    Projects = e.EmployeesProjects.Select(ep => ep.Project.Name)
                                                    .OrderBy(pn => pn)
                                                    .ToList()
                })
                .FirstOrDefault();

            var sb = new StringBuilder();
            sb.AppendLine($"{employee147.FirstName} {employee147.LastName} - {employee147.JobTitle}");

            foreach (var project in employee147.Projects)
            {
                sb.AppendLine(project);
            }
            return sb.ToString().TrimEnd();
        }

        public static string GetDepartmentsWithMoreThan5Employees(SoftUniContext context)
        {
            var departments = context.Departments
                .Where(d => d.Employees.Count() > 5)
                .OrderBy(d=> d.Employees.Count())
                .ThenBy(d=>d.Name)
                .Select(d => new
                {
                    departmentName = d.Name,
                    managerFirstName = d.Manager.FirstName,
                    managerLastName = d.Manager.LastName,
                    employees = d.Employees.OrderBy(e=>e.FirstName)
                                            .ThenBy(e=>e.LastName)
                                            .Select(e=> new { 
                                                e.FirstName,
                                                e.LastName,
                                                e.JobTitle
                                            })
                                            .ToList(),
                })
                .ToList();

            var sb = new StringBuilder();

            foreach (var department in departments)
            {
                sb.AppendLine($"{department.departmentName} - {department.managerFirstName} {department.managerLastName}");

                foreach (var employee in department.employees)
                {
                    sb.AppendLine($"{employee.FirstName} {employee.LastName} - {employee.JobTitle}");
                }
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetLatestProjects(SoftUniContext context)
        {
            var projects = context.Projects
                .OrderByDescending(p => p.StartDate)
                .Take(10)
                .OrderBy(p => p.Name)
                .Select(p => new
                {
                    p.Name,
                    p.Description,
                    p.StartDate,
                })
                .ToList();

            var sb = new StringBuilder();

            foreach (var project in projects)
            {
                sb.AppendLine(project.Name);
                sb.AppendLine(project.Description);
                sb.AppendLine(project.StartDate.ToString());
            }
            return sb.ToString().TrimEnd();
        }

        public static string IncreaseSalaries(SoftUniContext context)
        {
            var employees = context.Employees
                .Where(e => e.Department.Name == "Engineering" || e.Department.Name == "Tool Design" ||
                            e.Department.Name == "Marketing" || e.Department.Name == "Information Services")
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName);

            var sb = new StringBuilder();

            foreach (var employee in employees)
            {
                employee.Salary *= 1.12m;
                sb.AppendLine($"{employee.FirstName} {employee.LastName} (${employee.Salary:F2})");
            }

            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string GetEmployeesByFirstNameStartingWithSa(SoftUniContext context)
        {
            var employees = context.Employees
                .Where(e => e.FirstName.StartsWith("Sa"))
                .Select(e => new
                {
                    e.FirstName,
                    e.LastName,
                    e.JobTitle,
                    e.Salary,
                })
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .ToList();

            var sb = new StringBuilder();

            if (context.Employees.Any(e => e.FirstName == "Svetlin"))
            {
                var employeesByNamePattern = context.Employees
                    .Where(employee => employee.FirstName.StartsWith("SA"));

                foreach (var employeeByPattern in employeesByNamePattern)
                {
                    sb.AppendLine($"{employeeByPattern.FirstName} {employeeByPattern.LastName} " +
                                       $"- {employeeByPattern.JobTitle} - (${employeeByPattern.Salary})");
                }
            }
            else
            {
                foreach (var employee in employees)
                {
                    sb.AppendLine($"{employee.FirstName} {employee.LastName} - {employee.JobTitle} - (${employee.Salary:F2})");
                }
            }

            return sb.ToString().TrimEnd();
        }

        public static string DeleteProjectById(SoftUniContext context)
        {
            var empoyeesProjects = context.EmployeesProjects.Where(ep => ep.ProjectId == 2);
            var project = context.Projects.Where(p => p.ProjectId == 2).FirstOrDefault();

            context.EmployeesProjects.RemoveRange(empoyeesProjects);
            context.Projects.Remove(project);

            context.SaveChanges();

            var projects = context.Projects
                .Take(10)
                .Select(p => new
                {
                    p.Name
                }).ToList();

            var sb = new StringBuilder();

            foreach (var p in projects)
            {
                sb.AppendLine(p.Name);
            }

            return sb.ToString().TrimEnd();
        }

        public static string RemoveTown(SoftUniContext context)
        {
            var employees = context.Employees
                .Where(e => e.Address.Town.Name == "Seattle");

            var addresses = context.Addresses
                .Where(a => a.Town.Name == "Seattle");

            var town = context.Towns
                .Where(t => t.Name == "Seattle")
                .FirstOrDefault();

            foreach (var employee in employees)
            {
                employee.AddressId = null;
            }

            var count = addresses.Count();

            context.Addresses.RemoveRange(addresses);
            context.Towns.Remove(town);

            context.SaveChanges();

            return $"{count} addresses in Seattle were deleted";
        }
    }
}
