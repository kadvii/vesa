using AutoMapper;
using eVisaPlatform.Application.DTOs.Document;
using eVisaPlatform.Application.DTOs.Notification;
using eVisaPlatform.Application.DTOs.User;
using eVisaPlatform.Application.DTOs.Visa;
using eVisaPlatform.Application.DTOs.Family;
using eVisaPlatform.Application.DTOs.Support;
using eVisaPlatform.Application.DTOs.Consultants;
using eVisaPlatform.Application.DTOs.Agents;
using eVisaPlatform.Application.DTOs.Guarantee;
using eVisaPlatform.Application.DTOs.Payment;
using eVisaPlatform.Application.DTOs.Appointment;
using eVisaPlatform.Application.DTOs.AuditLog;
using eVisaPlatform.Application.DTOs.Setting;
using eVisaPlatform.Domain.Entities;

namespace eVisaPlatform.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User
        CreateMap<User, UserResponseDto>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));

        // VisaApplication
        CreateMap<VisaApplication, VisaApplicationResponseDto>()
            .ForMember(dest => dest.VisaType, opt => opt.MapFrom(src => src.VisaType.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.ApplicantName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : string.Empty))
            .ForMember(dest => dest.DestinationCountry, opt => opt.MapFrom(src => src.DestinationCountry))
            .ForMember(dest => dest.ApplicantFullName, opt => opt.MapFrom(src => src.ApplicantFullName))
            .ForMember(dest => dest.Nationality, opt => opt.MapFrom(src => src.Nationality))
            .ForMember(dest => dest.IntendedTravelDate, opt => opt.MapFrom(src => src.IntendedTravelDate));

        // Document
        CreateMap<Document, DocumentResponseDto>()
            .ForMember(dest => dest.FileType, opt => opt.MapFrom(src => src.FileType.ToString()));

        // Notification
        CreateMap<Notification, NotificationResponseDto>();

        // Family
        CreateMap<FamilyMember, FamilyMemberResponseDto>();

        // Support
        CreateMap<SupportTicket, SupportTicketResponseDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
        CreateMap<TicketReply, TicketReplyResponseDto>();

        // Travel Consultant
        CreateMap<TravelConsultant, TravelConsultantResponseDto>();

        // Visa Agent
        CreateMap<VisaAgent, VisaAgentResponseDto>();

        // Visa Guarantee
        CreateMap<GuaranteeRequest, GuaranteeRequestResponseDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        // Payment
        CreateMap<Payment, PaymentResponseDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Method, opt => opt.MapFrom(src => src.Method.ToString()));

        // Appointment
        CreateMap<Appointment, AppointmentResponseDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        // AuditLog
        CreateMap<AuditLog, AuditLogResponseDto>();

        // SystemSetting
        CreateMap<SystemSetting, SettingResponseDto>();
    }
}
