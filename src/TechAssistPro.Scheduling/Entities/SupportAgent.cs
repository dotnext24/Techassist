using TechAssistPro.SharedKernel.Domain;

namespace TechAssistPro.Scheduling.Entities
{
    public sealed class SupportAgent : AggregateRoot
    {
        public Guid SupportAgentId => Id;
        public string Name { get; private set; } = default!;
        public IReadOnlyCollection<Skill> Skills => _skills;
        public AgentAvailability Availability { get; private set; } = default!;
        public int ActiveAssignments { get; private set; } = default!;

        private readonly List<Skill> _skills = new();

        private SupportAgent() { }

        public SupportAgent(Guid id, string name, IEnumerable<Skill> skills)
            : base(id)
        {
            Name = name;
            _skills.AddRange(skills);
            Availability = AgentAvailability.Available();
        }

        public static SupportAgent Create(string name, IEnumerable<Skill> skills, string createdBy)
        {
            Guid id = Guid.NewGuid();
            var agent = new SupportAgent(
                id,
                name,
                skills);
            agent.Touch(createdBy);

            //Raise domain events if any

            return agent;
        }


        public void Update(string name, string updatedBy)
        {
            Name = name;
            Touch(updatedBy);
        }

        public bool CanHandle(string category)
            => true //Availability.IsAvailable
               && _skills.Any(s => s.Category == category);

        public void Assign()
        {
            ActiveAssignments++;
            Availability = AgentAvailability.Unavailable();
        }

        private void Touch(string updatedBy)
        {
            LastUpdatedAtUtc = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }


    }

}