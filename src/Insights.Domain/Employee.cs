namespace Insights.Domain
{
    public class Employee
    {
        public EmployeeId EmployeeId { get; private set; }
        public string PreferredName { get; private set; }
        public string FamilyName { get; private set; }
        public string GivenName { get; private set; }
        public Gender Gender { get; private set; }

        public JobTitle JobTitle { get; private set; }
        public Department Department { get; private set; }
    }

    public class Department
    {
    }

    public class JobTitle
    {
    }
}
