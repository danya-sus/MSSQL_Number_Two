using Microsoft.EntityFrameworkCore;
using MSSQL_Number_Two.Data.Models;
using System;
using System.Linq;
using System.Text;

namespace MSSQL_Number_Two
{
    class Program
    {
        static private EmployeesContext _context = new EmployeesContext();

        static void Main(string[] args)
        {
            //3, 16 is smallest. 2, 5, 12 can use.
            //Console.WriteLine(GetEmployeesInformation());
            //Console.WriteLine(HighlyPaidEmployees());
            //Console.WriteLine(RelocationEmployees());
            //Console.WriteLine(ProjectAudit());
            //Console.WriteLine(DossierOnEmployee(1));
            //Console.WriteLine(SmallDepartments());
            //Console.WriteLine(SalaryIncrease(3, 15));
            //Console.WriteLine(CleanTheLogs(12));
            //Console.WriteLine(City404(32));
        }

        static string GetEmployeesInformation()
        {
            var employees = _context.Employees
                .OrderBy(e => e.EmployeeId)
                .Select(e => e)
                .ToList();

            var sb = new StringBuilder();

            foreach(var e in employees)
            {
                sb.AppendLine($"{e.FirstName} {e.LastName} {e.MiddleName} {e.JobTitle}");
            }

            return sb.ToString().TrimEnd();
        }

        static string HighlyPaidEmployees()
        {
            var employees = _context.Employees
                .Where(e => e.Salary > 48000)
                .OrderBy(e => e.LastName)
                .Select(e => new
                {
                    e.FirstName,
                    e.LastName,
                    e.MiddleName,
                    e.Salary
                })
                .ToList();

            var sb = new StringBuilder();

            foreach(var e in employees)
            {
                sb.AppendLine($"{e.FirstName} {e.LastName} {e.MiddleName} {e.Salary}");
            }

            return sb.ToString().TrimEnd();
        }

        static string RelocationEmployees()
        {
            string newAddress = "221B Baker Street";
            
            var address = new Addresses()
            {
                AddressText = newAddress
            };

            _context.Addresses.Add(address);
            _context.SaveChanges();
            Console.WriteLine($"The address {newAddress} has been added");

            var employees = _context.Employees
                .Where(e => e.LastName == "Brown")
                .ToList();
            foreach (var e in employees)
            {
                e.Address = address;
            }
            _context.SaveChanges();

            employees = _context.Employees
                .Where(e => e.LastName == "Brown")
                .Select(e => e)
                .ToList();

            var sb = new StringBuilder();
            foreach (var e in employees)
            {
                sb.AppendLine($"{e.FirstName} {e.LastName} {e.MiddleName} {e.JobTitle} {e.DepartmentId} {e.ManagerId} {e.HireDate} {e.Salary} {e.AddressId}");
            }

            return sb.ToString().TrimEnd();
        }

        static string ProjectAudit()
        {
            using (var db = new EmployeesContext())
            {
                DateTime startDate = DateTime.Parse("2002/01/01 00:00:00");
                DateTime endDate = DateTime.Parse("2006/01/01 00:00:00");

                var employees = db.Projects
                    .Where(p => p.StartDate >= startDate)
                    .Where(p => p.StartDate < endDate)
                    .Join(db.EmployeesProjects,
                    p => p.ProjectId,
                    ep => ep.ProjectId,
                    (p, ep) => new
                    {
                        ep.EmployeeId
                    })
                    .Join(db.Employees,
                    ep => ep.EmployeeId,
                    e => e.EmployeeId,
                    (ep, e) => new
                    {
                        ep.EmployeeId,
                        e.FirstName,
                        e.LastName
                    })
                    .Distinct()
                    .Take(5)
                    .ToList();

                var sb = new StringBuilder();

                for (int i = 0; i < employees.Count(); i++)
                {
                    sb.AppendLine($"\n----------------------\nEmployeer: {employees[i].FirstName} {employees[i].LastName}");

                    var information = db.Employees
                        .Where(e => e.EmployeeId == employees[i].EmployeeId)
                        .Join(db.Employees,
                        e1 => e1.ManagerId,
                        e2 => e2.EmployeeId,
                        (e1, e2) => new
                        {
                            e1.EmployeeId,
                            e2.FirstName,
                            e2.LastName,
                        })
                        .Join(db.EmployeesProjects,
                        e => e.EmployeeId,
                        ep => ep.EmployeeId,
                        (e, ep) => new
                        {
                            e.FirstName,
                            e.LastName,
                            ep.ProjectId
                        })
                        .Join(db.Projects,
                        e => e.ProjectId,
                        p => p.ProjectId,
                        (e, p) => new
                        {
                            e.FirstName,
                            e.LastName,
                            p.Name,
                            p.StartDate,
                            p.EndDate
                        })
                        .Where(e => e.StartDate >= startDate)
                        .Where(e => e.StartDate < endDate)
                        .ToList();

                    sb.AppendLine($"Manager: {information[0].FirstName} {information[0].LastName}");
                    sb.AppendLine("\nProjects:");

                    foreach (var item in information)
                    {
                        sb.AppendLine($"Name: {item.Name}.");
                        sb.Append($"Beginning: {item.StartDate}; ");

                        if (item.EndDate != null) sb.AppendLine($"The end: {item.EndDate}.");
                        else sb.AppendLine("Not completed.");
                    }
                }
                return sb.ToString().TrimEnd();
            }
        }

        static string DossierOnEmployee(int id)
        {
            using (var db = new EmployeesContext())
            {
                var employees = db.Employees
                    .Where(e => e.EmployeeId == id)
                    .Join(db.EmployeesProjects,
                    e => e.EmployeeId,
                    ep => ep.EmployeeId,
                    (e, ep) => new
                    {
                        e.FirstName,
                        e.LastName,
                        e.MiddleName,
                        e.JobTitle,
                        ep.EmployeeId,
                        ep.ProjectId
                    })
                    .Join(db.Projects,
                    e => e.ProjectId,
                    p => p.ProjectId,
                    (e, p) => new
                    {
                        e.FirstName,
                        e.LastName,
                        e.MiddleName,
                        e.JobTitle,
                        p.Name
                    })
                    .ToList();


                var sb = new StringBuilder();

                sb.AppendLine("Employee: ");
                sb.AppendLine($"{employees[0].FirstName} {employees[0].LastName} {employees[0].MiddleName}; {employees[0].JobTitle}");

                sb.AppendLine("\nProjects: ");
                foreach (var e in employees)
                {
                    sb.AppendLine($"{e.Name}");
                }

                return sb.ToString().TrimEnd();
            }
        }

        static string SmallDepartments()
        {
            using (var db = new EmployeesContext())
            {
                var departments = db.Departments
                    .Join(db.Employees,
                    d => d.DepartmentId,
                    e => e.DepartmentId,
                    (d, e) => new
                    {
                        d.Name,
                        e.EmployeeId
                    })
                    .GroupBy(
                    d => d.Name,
                    c => c.EmployeeId,
                    (d, c) => new
                    {
                        Name = d,
                        count = c.Count()
                    })
                    .Where(d => d.count < 5)
                    .ToList();

                var sb = new StringBuilder();

                foreach (var d in departments)
                {
                    sb.AppendLine($"{d.Name}: {d.count}");
                }

                return sb.ToString().TrimEnd();
            }
        }

        static string SalaryIncrease(int department_id, decimal percent)
        {
            using (var db = new EmployeesContext())
            {
                var employees = db.Employees
                    .Where(e => e.DepartmentId == department_id)
                    .Select(e => e)
                    .ToList();

                foreach (var e in employees)
                {
                    e.Salary = e.Salary * (1 + (percent / 100));
                }

                db.SaveChanges();

                var sb = new StringBuilder();
                foreach (var e in employees)
                {
                    sb.AppendLine($"{e.Salary}");
                }

                return sb.ToString().TrimEnd();
            }
        }

        static string CleanTheLogs(int department_id)
        {
            using (var db = new EmployeesContext())
            {
                var departments = db.Departments
                    .Select(d => d)
                    .ToList();

                var newDepartment = new Departments();

                foreach (var d in departments)
                {
                    if (d.DepartmentId == department_id)
                    {
                        d.ManagerId = null;
                        db.SaveChanges();
                    }
                    else newDepartment = d;
                }

                var employeesDepartment = db.Employees
                    .Where(e => e.DepartmentId == department_id)
                    .ToList();

                foreach (var ep in employeesDepartment)
                {
                    ep.DepartmentId = newDepartment.DepartmentId;
                    ep.ManagerId = newDepartment.ManagerId;
                }
                db.SaveChanges();

                var department = db.Departments
                    .First(d => d.DepartmentId == department_id);

                db.Remove(department);
                db.SaveChanges();

                return $"The department {department_id} has been removed.";
            }
        }

        static string City404(int town_id)
        {
            using (var db = new EmployeesContext())
            {
                var addresses = db.Addresses
                    .Where(a => a.TownId == town_id)
                    .Select(a => a);
                foreach (var a in addresses)
                {
                    a.TownId = null;
                }
                db.SaveChanges();

                var town = db.Towns.First(t => t.TownId == town_id);
                db.Towns.Remove(town);
                db.SaveChanges();

                return $"Town {town_id} almost removed.";
            }
        }
    }
}
