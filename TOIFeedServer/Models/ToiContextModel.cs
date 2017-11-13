using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading;
using Microsoft.AspNetCore.Hosting.Internal;

namespace TOIFeedServer.Models
{
    public class ToiContextModel
    {
        public Guid ToiId { get; set; }
        [ForeignKey(nameof(ToiId))]
        public ToiModel Toi { get; set; }
        
        public Guid ContextId { get; set; }
        [ForeignKey(nameof(ContextId))]
        public ContextModel Context { get; set; }

        public ToiContextModel(ToiModel toi, ContextModel ctx)
        {
            Toi = toi;
            ToiId = toi.Id;
            ContextId = ctx.Id;
            Context = ctx;
        }

        public ToiContextModel(){}
    }
}