using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotesApp.Application.Response
{
    public class ServiceResponse<T>
    {
        public bool Success { get; set; } = false;
        public string? Message { get; set; } = null;
        public T Data { get; set; }
    }
}
