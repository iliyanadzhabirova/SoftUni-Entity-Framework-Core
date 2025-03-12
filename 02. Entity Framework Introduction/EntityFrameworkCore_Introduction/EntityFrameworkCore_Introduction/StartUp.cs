using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SoftUni.Data;
using SoftUni.Models;
using System.Text;

namespace SoftUni
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            using SoftUniContext dbContext = new SoftUniContext();
            dbContext.Database.EnsureCreated();

            string result = GetEmployeesFromResearchAndDevelopment(dbContext);
            Console.WriteLine(result);
        }
        //Problem 03
        public static string GetEmployeesFullInformation(SoftUniContext context)
        {
            StringBuilder sb = new StringBuilder();
            var employees = context
                .Employees
                .OrderBy(e => e.EmployeeId)
                .Select(e => new
                {
                    e.FirstName,
                    e.LastName,
                    e.MiddleName,
                    e.JobTitle,
                    e.Salary

                })
                .ToArray(); // Materialize the query -> The query is detached from DB
                            // The data is now in-memory -> In the memory of our C# Program 

            // Hint: It is possible to attach to the DB at later point
            foreach (var e in employees)
            {
                sb
                    .AppendLine($"{e.FirstName} {e.LastName} {e.MiddleName} {e.JobTitle} {e.Salary.ToString("F2")}");

            }
                
            return sb.ToString().TrimEnd();
        }
        //Problem 04
        public static string GetEmployeesWithSalaryOver50000(SoftUniContext context)
        {
            StringBuilder sb = new StringBuilder();
            var employees = context
               .Employees
               .Where(e => e.Salary > 50000)
               .Select(e => new
               {
                   e.FirstName,
                   e.Salary
               })
               .OrderBy(e => e.FirstName)
               .ToArray();

            foreach (var e in employees)
            {
                sb
                    .AppendLine($"{e.FirstName} - {e.Salary.ToString("F2")}");
            }
            return sb.ToString().TrimEnd();

        }
        //Problem 05

        public static string GetEmployeesFromResearchAndDevelopment(SoftUniContext context)
        {
            StringBuilder sb = new StringBuilder();

            var employeesRnD = context
                .Employees
                .Where(e => e.Department.Name.Equals("Research and Development"))
                .Select(e => new
                {
                    e.FirstName,
                    e.LastName,
                    DepartmentName = e.Department.Name,
                    e.Salary
                })
                .OrderBy(e => e.Salary)
                .ThenByDescending(e => e.FirstName)
                .ToArray();

            foreach (var e in employeesRnD)
            {
                sb
                    .AppendLine($"{e.FirstName} {e.LastName} from Research and Development - ${e.Salary.ToString("F2")}");
            }
            return sb.ToString().TrimEnd();
        }

        // Problem 06

        public static string AddNewAddressToEmployee(SoftUniContext context)
        {
            const string newAddressText = "Vitoshka 15"; // Just good practice to avoid magic strings and numbers
            const int newAddressTownId = 4;

            Address newAddress = new Address()
            {
                AddressText = newAddressText,
                TownId = newAddressTownId
            };

            // 2 ways for adding the new address
            // I. Explicitly
            //  1. First add the new address to addresses
            //  2. Attach the new address to  employee
            //  3. SaveChanges
            // II. Implicitly (using nested add)
            //  1. Attach the new address to employee
            //  2. SaveChanges 

            //context.Addresses.Add(newAddress); // 1 from Explicit

            Employee nakovEmployee = context
                .Employees
                .First(e => e.LastName.Equals("Nakov"));

            nakovEmployee.Address = newAddress; // 2 from Explicit / 1 from Implicit

            context.SaveChanges(); // 3 from Explicit /2 from Implicit 

            string?[] addresses = context
                .Employees
                .Where(e => e.AddressId.HasValue)
                .OrderByDescending(e => e.AddressId)
                .Select(e => e.Address!.AddressText)
                .Take(10)
                .ToArray();

            return String.Join(Environment.NewLine, addresses);
        }

        // Problem 07

        public static string GetEmployeesInPeriod(SoftUniContext context)
        {
            StringBuilder sb = new StringBuilder();

            var employeesWithProjects = context
                .Employees
                .Select(e => new
                {
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    ManagerFirstName = e.Manager == null ?
                        null : e.Manager.FirstName,
                    ManagerLastName = e.Manager == null ?
                        null : e.Manager.LastName,
                    Projects = e.EmployeesProjects
                        .Select(ep => ep.Project)
                        .Where(p => p.StartDate.Year >= 2001 &&
                                    p.StartDate.Year <= 2003)
                        .Select(p => new
                        {
                            ProjectName = p.Name,
                            p.StartDate,
                            p.EndDate
                        })
                        .ToArray()
                })
                .Take(10)
                .ToArray();

            foreach (var e in employeesWithProjects)
            {
                sb
                    .AppendLine($"{e.FirstName} {e.LastName} - Manager: {e.ManagerFirstName} {e.ManagerLastName}");
                foreach (var p in e.Projects)
                {
                    string startDateFormatted = p.StartDate
                        .ToString("M/d/yyyy h:mm:ss tt");
                    string endDateFormatted = p.EndDate.HasValue ?
                        p.EndDate.Value.ToString("M/d/yyyy h:mm:ss tt") : "not finished";
                    sb
                        .AppendLine($"--{p.ProjectName} - {startDateFormatted} - {endDateFormatted}");
                }
            }

            return sb.ToString().TrimEnd();
        }
        //Problem 08
        public static string GetAddressesByTown(SoftUniContext context)
        {
            var sb = new StringBuilder();

            var addresses = context.Addresses
                .OrderByDescending(a => a.Employees.Count)
                .ThenBy(a => a.Town!.Name)
                .ThenBy(a => a.AddressText)
                .Take(10)
                .Select(a => new
                {
                    AddressText = a.AddressText,
                    TownName = a.Town!.Name,
                    EmployeeCount = a.Employees.Count
                })
                .ToList();

            foreach (var a in addresses)
            {
                sb.AppendLine($"{a.AddressText}, {a.TownName} - {a.EmployeeCount} employees");
            }

            return sb.ToString().Trim();
        }

        //Problem 09
        public static string GetEmployee147(SoftUniContext context)
        {
            var sb = new StringBuilder();

            var employee = context.Employees
                .Where(e => e.EmployeeId == 147)
                .Select(e => new
                {
                    e.FirstName,
                    e.LastName,
                    e.JobTitle,
                    Projects = e.EmployeesProjects.Select(p => new
                    {
                        ProjectName = p.Project.Name
                    })
                })
                .ToList(); 

            foreach (var e in employee)
            {
                sb.AppendLine($"{e.FirstName} {e.LastName} - {e.JobTitle}");

                foreach (var p in e.Projects.OrderBy(p => p.ProjectName))
                {
                    sb.AppendLine(p.ProjectName);
                }
            }

            return sb.ToString().Trim();
        }

        //Problem 10
        public static string GetDepartmentsWithMoreThan5Employees(SoftUniContext context)
        {
            var sb = new StringBuilder();

            var departments = context.Departments
                .Where(d => d.Employees.Count > 5)
                .OrderBy(d => d.Employees.Count)
                .ThenBy(d => d.Name)
                .Select(d => new
                {
                    d.Name,
                    ManagerFirstName = d.Manager.FirstName,
                    ManagerLastName = d.Manager.LastName,
                    Employees = d.Employees
                        .OrderBy(e => e.FirstName)
                        .ThenBy(e => e.LastName)
                        .Select(e => new
                        {
                            e.FirstName,
                            e.LastName,
                            e.JobTitle
                        })
                        .ToList()
                })
                .ToList(); 

            foreach (var d in departments)
            {
                sb.AppendLine($"{d.Name} - {d.ManagerFirstName} {d.ManagerLastName}");

                foreach (var employee in d.Employees)
                {
                    sb.AppendLine($"{employee.FirstName} {employee.LastName} - {employee.JobTitle}");
                }
            }

            return sb.ToString().Trim();
        }

        //Problem 11
        public static string GetLatestProjects(SoftUniContext context)
        {
            var sb = new StringBuilder();

            var projects = context.Projects
                .OrderByDescending(p => p.StartDate)
                .Take(10)
                .OrderBy(p => p.Name)
                .Select(p => new
                {
                    p.Name,
                    p.Description,
                    StartDate = p.StartDate.ToString("M/d/yyyy h:mm:ss tt") 
                })
                .ToList();

            foreach (var p in projects)
            {
                sb.AppendLine(p.Name);
                sb.AppendLine(p.Description);
                sb.AppendLine(p.StartDate);
            }

            return sb.ToString().Trim();
        }
        //Problem 12
        public static string IncreaseSalaries(SoftUniContext context)
        {
            var sb = new StringBuilder();

            var employees = context.Employees
                .Where(e =>
                e.Department.Name == "Engineering"
                || e.Department.Name == "Tool Design"
                || e.Department.Name == "Marketing"
                || e.Department.Name == "Information Services")
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .ToList();

            foreach (var e in employees)
            {
                e.Salary *= 1.12m;
                sb.AppendLine($"{e.FirstName} {e.LastName} (${e.Salary:f2})");
            }

            context.SaveChanges();

            return sb.ToString().Trim();
        }
        //Problem 13
        public static string GetEmployeesByFirstNameStartingWithSa(SoftUniContext context)
        {
            var sb = new StringBuilder();

            var employees = context.Employees
                .Where(e => e.FirstName.StartsWith("Sa"))
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .ToList();

            foreach (var e in employees)
            {
                sb.AppendLine($"{e.FirstName} {e.LastName} - {e.JobTitle} - (${e.Salary:f2})");
            }

            return sb.ToString().Trim();
        }
        //Problem 14
        public static string DeleteProjectById(SoftUniContext context)
        {
            const int deleteProjectId = 2; // Good practice

            IEnumerable<EmployeeProject> employeeProjectsDelete = context
                .EmployeesProjects
                .Where(ep => ep.ProjectId == deleteProjectId)
                .ToArray();
            context.EmployeesProjects.RemoveRange(employeeProjectsDelete);

            Project? deleteProject = context
                .Projects
                .Find(deleteProjectId);
            if (deleteProject != null)
            {
                context.Projects.Remove(deleteProject);
            }

            context.SaveChanges();

            string[] projectNames = context
                .Projects
                .Select(p => p.Name)
                .Take(10)
                .ToArray();

            return String.Join(Environment.NewLine, projectNames);
        }
        //Problem 15
        public static string RemoveTown(SoftUniContext context)
        {
            var sb = new StringBuilder();

            int deletedAddressesCount = 0;

            var townToBeDeleted = context.Towns
                .FirstOrDefault(t => t.Name == "Seattle");

            var addressesToBeRemoved = context.Addresses
                .Where(a => a.Town!.Name == "Seattle")
                .ToList();

            var employeesToBeEdited = context.Employees
                .Where(e => e.Address!.Town!.Name == "Seattle");

            foreach (var employee in employeesToBeEdited)
            {
                employee.Address = null;
            }

            foreach (var address in addressesToBeRemoved)
            {
                context.Addresses.Remove(address);
                deletedAddressesCount++;
            }

            context.Towns.Remove(townToBeDeleted);

            context.SaveChanges();

            sb.AppendLine($"{deletedAddressesCount} addresses in Seattle were deleted");

            return sb.ToString().Trim();
        }

        private static void RestoreDatabase(SoftUniContext context)
        {
            // This will NOT insert data in the DB
            // This would work if we were working Code-First approach
            // This is not optimal for DB-First
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }

    }
}
