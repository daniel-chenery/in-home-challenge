using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using TT.Deliveries.Business.Models;

namespace TT.Deliveries.Business
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Data.Models.Delivery, Delivery>();
            CreateMap<Data.Models.AccessWindow, AccessWindow>()
                .ConstructUsing(aw => new AccessWindow(aw.StartTime, aw.EndTime));
            CreateMap<Data.Models.Order, Order>()
                .ConstructUsing(o => new Order(o.Id, o.Sender ?? string.Empty));
            CreateMap<Data.Models.Recipient, Recipient>()
                .ConstructUsing(r => new Recipient(r.Name!, r.Address ?? string.Empty, r.Email ?? string.Empty, r.PhoneNumber ?? string.Empty));
        }
    }
}