using PartnersHub.InnovationHub.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.InnovationHub.Domain.Aggregates.ProfileDirectory
{
    public class SponsorProfile : AggregateRoot
    {
        private SponsorProfile() { }

        public SponsorProfile(Guid id, string name)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("id is required.", nameof(id));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name is required.", nameof(name));

            Id = id;
            Name = name.Trim();
            CreatedAt = DateTime.UtcNow;
        }

        public string Name { get; private set; }
    }
}