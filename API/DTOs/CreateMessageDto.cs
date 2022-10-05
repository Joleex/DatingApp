using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.DTOs
{
    public class CreateMessageDto
    {   /*this takes care of the setup for the DTO, the entity, the repository1*/
        public string RecipientUsername { get; set; }
        public string Content { get; set; }
    }
}