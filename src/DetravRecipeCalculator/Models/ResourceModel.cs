using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DetravRecipeCalculator.Models
{
 
    public class ResourceModel
    {
        public bool IsEnabled { get; set; }
        public string? Name { get; set; }
        public string? BackgroundColor { get; set; }
        public string? ForegroundColor { get; set; }
        public string? ConnectorColor { get; set; }
        public byte[]? Icon { get; set; }
    }
}
