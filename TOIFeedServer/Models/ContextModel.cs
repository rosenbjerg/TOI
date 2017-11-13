﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace TOIFeedServer.Models
{
    public class ContextModel
    {
        public ContextModel()
        {
            
        }
        public ContextModel(string id, string title, string description = null)
        {
            Id = id;
            Title = title;
            Description = description;
        }

        [Key]
        public string Id { get; set; }

        public string Description { get; set; }

        [StringLength(70)]
        [Required]
        public string Title { get; set; }

        public override bool Equals(object obj)
        {
            return obj != null && obj is ContextModel t && t.Id == Id;
        }

        protected bool Equals(ContextModel other)
        {
            return Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        
    }
}
