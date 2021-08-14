using AO.Models;
using AO.Models.Enums;
using AO.Models.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;

namespace BlazorAO.Models.Conventions
{
    public abstract class BaseTable : IModel<int>
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        [SaveAction(SaveAction.Insert)]
        public string CreatedBy { get; set; }

        [SaveAction(SaveAction.Insert)]
        public DateTime DateCreated { get; set; }

        [MaxLength(50)]
        [SaveAction(SaveAction.Update)]
        public string ModifiedBy { get; set; }

        [SaveAction(SaveAction.Update)]
        public DateTime? DateModified { get; set; }
    }
}
