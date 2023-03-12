using System;
using System.ComponentModel.DataAnnotations;

namespace ToDoListWebAplication.Models
{
    public class ToDoItem
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; }
        public DateTime DateTime { get; set; }
        public bool IsDone { get; set; }

    }
}
