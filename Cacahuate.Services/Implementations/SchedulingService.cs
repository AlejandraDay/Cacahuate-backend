using Cacahuate.DataAccess.Entities;
using Cacahuate.DataAccess.Repositories;
using Cacahuate.Services.Interfaces;
using Cacahuate.Shared.DTOs.Scheduling;
using Cacahuate.Shared.Enums;

namespace Cacahuate.Services.Implementations;

public class SchedulingService(
    ITherapistRepository therapistRepository,
    IParentRepository parentRepository,
    IPatientRepository patientRepository,
    IAvailabilityRepository availabilityRepository,
    IAppointmentRepository appointmentRepository,
    INotificationRepository notificationRepository,
    IUserRepository userRepository,
    IRatingRepository ratingRepository,
    IEmailService emailService) : ISchedulingService
{
    // â"€â"€ Availability â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€

    public async Task<AvailabilityResponse> AddAvailabilityAsync(Guid therapistUserId, AvailabilityRequest request)
    {
        var therapist = await therapistRepository.GetByUserIdAsync(therapistUserId)
            ?? throw new KeyNotFoundException("Therapist not found.");

        if (request.EndTime <= request.StartTime)
            throw new ArgumentException("End time must be after start time.");

        if (request.SessionDurationMinutes <= 0)
            throw new ArgumentException("Session duration must be greater than 0.");

        var totalMinutes = (request.EndTime - request.StartTime).TotalMinutes;
        if (totalMinutes < request.SessionDurationMinutes)
            throw new ArgumentException("Time block must be at least as long as the session duration.");

        var availability = new TherapistAvailability
        {
            TherapistId = therapist.Id,
            Date = request.Date,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            SessionDurationMinutes = request.SessionDurationMinutes
        };

        await availabilityRepository.AddAsync(availability);
        await availabilityRepository.SaveChangesAsync();

        return MapToAvailabilityResponse(availability, therapist, []);
    }

    public async Task<List<AvailabilityResponse>> GetTherapistAvailabilityAsync(Guid therapistId)
    {
        var therapist = await therapistRepository.GetByIdAsync(therapistId)
            ?? throw new KeyNotFoundException("Therapist not found.");

        var availabilities = await availabilityRepository.GetByTherapistAsync(therapistId);

        return availabilities.Select(a =>
        {
            var slots = BuildTimeSlots(a);
            return MapToAvailabilityResponse(a, therapist, slots);
        }).ToList();
    }

    public async Task<List<AvailabilityResponse>> GetMyAvailabilityAsync(Guid therapistUserId)
    {
        var therapist = await therapistRepository.GetByUserIdAsync(therapistUserId)
            ?? throw new KeyNotFoundException("Therapist not found.");

        return await GetTherapistAvailabilityAsync(therapist.Id);
    }

    public async Task<AvailabilityResponse> GetAvailabilityWithSlotsAsync(Guid availabilityId)
    {
        var availability = await availabilityRepository.GetByIdAsync(availabilityId)
            ?? throw new KeyNotFoundException("Availability block not found.");

        var slots = BuildTimeSlots(availability);
        return MapToAvailabilityResponse(availability, availability.Therapist, slots);
    }

    // â"€â"€ Appointments â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€

    public async Task<AppointmentResponse> BookAppointmentAsync(Guid parentUserId, BookAppointmentRequest request)
    {
        var parent = await parentRepository.GetByUserIdAsync(parentUserId)
            ?? throw new KeyNotFoundException("Parent not found.");

        var patient = await patientRepository.GetByIdAsync(request.PatientId)
            ?? throw new KeyNotFoundException("Patient not found.");

        if (patient.ParentId != parent.Id)
            throw new UnauthorizedAccessException("Patient does not belong to this parent.");

        var therapist = await therapistRepository.GetByIdAsync(request.TherapistId)
            ?? throw new KeyNotFoundException("Therapist not found.");

        var availabilities = await availabilityRepository.GetByTherapistAndDateAsync(therapist.Id, request.Date);
        var matchingBlock = availabilities.FirstOrDefault(a =>
            a.StartTime <= request.StartTime &&
            request.StartTime.AddMinutes(a.SessionDurationMinutes) <= a.EndTime);

        if (matchingBlock is null)
            throw new InvalidOperationException("Selected time slot is not within the therapist's availability.");

        var endTime = request.StartTime.AddMinutes(matchingBlock.SessionDurationMinutes);

        var alreadyBooked = await appointmentRepository.SlotIsBookedAsync(matchingBlock.Id, request.StartTime);
        if (alreadyBooked)
            throw new InvalidOperationException("This time slot is already booked.");

        var appointment = new Appointment
        {
            TherapistId = therapist.Id,
            PatientId = patient.Id,
            ParentId = parent.Id,
            AvailabilityId = matchingBlock.Id,
            Date = request.Date,
            StartTime = request.StartTime,
            EndTime = endTime,
            Status = AppointmentStatus.Pending,
            Notes = request.Notes
        };

        await appointmentRepository.AddAsync(appointment);
        await appointmentRepository.SaveChangesAsync();

        // Notify all admins
        var dateLabel = request.Date.ToString("dd/MM/yyyy");
        var therapistName = $"{therapist.User.FirstName} {therapist.User.LastName}";
        var patientName = $"{patient.FirstName} {patient.LastName}";
        var parentName = $"{parent.User.FirstName} {parent.User.LastName}";

        var admins = await userRepository.GetByRoleAsync(UserRole.Admin);
        var adminNotifications = admins.Select(admin => new Notification
        {
            UserId = admin.Id,
            Title = "Nueva cita pendiente",
            Message = $"{parentName} agendÃ³ una cita para {patientName} con {therapistName} el {dateLabel} a las {request.StartTime}.",
            AppointmentId = appointment.Id,
        }).ToList();

        await notificationRepository.AddRangeAsync(adminNotifications);
        await notificationRepository.SaveChangesAsync();

        foreach (var admin in admins)
        {
            await emailService.SendAsync(
                admin.Email, $"{admin.FirstName} {admin.LastName}",
                "Nueva cita pendiente de aprobaciÃ³n",
                BuildEmailHtml(
                    $"Hola {admin.FirstName},",
                    $"Se ha agendado una nueva cita que requiere tu aprobaciÃ³n.",
                    [
                        ("Paciente", patientName),
                        ("Terapeuta", therapistName),
                        ("Padre/Madre", parentName),
                        ("Fecha", dateLabel),
                        ("Hora", request.StartTime.ToString()),
                    ]
                )
            );
        }

        return new AppointmentResponse
        {
            Id = appointment.Id,
            TherapistId = therapist.Id,
            TherapistName = therapistName,
            PatientId = patient.Id,
            PatientName = patientName,
            Date = appointment.Date,
            StartTime = appointment.StartTime,
            EndTime = appointment.EndTime,
            Status = appointment.Status,
            Notes = appointment.Notes,
            CreatedAt = appointment.CreatedAt
        };
    }

    public async Task<List<AppointmentResponse>> GetAppointmentsByParentAsync(Guid parentUserId)
    {
        var parent = await parentRepository.GetByUserIdAsync(parentUserId)
            ?? throw new KeyNotFoundException("Parent not found.");

        var appointments = await appointmentRepository.GetByParentAsync(parent.Id);
        return appointments.Select(MapToAppointmentResponse).ToList();
    }

    public async Task<List<AppointmentResponse>> GetAppointmentsByTherapistAsync(Guid therapistUserId)
    {
        var therapist = await therapistRepository.GetByUserIdAsync(therapistUserId)
            ?? throw new KeyNotFoundException("Therapist not found.");

        var appointments = await appointmentRepository.GetByTherapistAsync(therapist.Id);
        return appointments.Select(MapToAppointmentResponse).ToList();
    }

    public async Task<AppointmentResponse> CancelAppointmentAsync(Guid appointmentId, Guid requestingUserId)
    {
        var appointment = await appointmentRepository.GetByIdAsync(appointmentId)
            ?? throw new KeyNotFoundException("Appointment not found.");

        var therapist = appointment.Therapist;
        var parent = appointment.Parent;

        bool isTherapist = therapist.UserId == requestingUserId;
        bool isParent = parent.UserId == requestingUserId;

        if (!isTherapist && !isParent)
            throw new UnauthorizedAccessException("You are not authorized to cancel this appointment.");

        if (appointment.Status == AppointmentStatus.Cancelled)
            throw new InvalidOperationException("Appointment is already cancelled.");

        appointment.Status = AppointmentStatus.Cancelled;
        await appointmentRepository.SaveChangesAsync();

        var dateLabel = appointment.Date.ToString("dd/MM/yyyy");
        var therapistName = $"{therapist.User.FirstName} {therapist.User.LastName}";
        var patientName = $"{appointment.Patient.FirstName} {appointment.Patient.LastName}";
        var parentName = $"{parent.User.FirstName} {parent.User.LastName}";

        if (isTherapist)
        {
            // Notify parent
            await notificationRepository.AddAsync(new Notification
            {
                UserId = parent.UserId,
                Title = "Cita cancelada",
                Message = $"Tu cita del {dateLabel} a las {appointment.StartTime} con {therapistName} fue cancelada por el terapeuta.",
                AppointmentId = appointment.Id,
            });
            await notificationRepository.SaveChangesAsync();

            await emailService.SendAsync(
                parent.User.Email, parentName,
                "Tu cita ha sido cancelada",
                BuildEmailHtml(
                    $"Hola {parent.User.FirstName},",
                    "Lamentamos informarte que tu cita ha sido cancelada por el terapeuta.",
                    [
                        ("Terapeuta", therapistName),
                        ("Paciente", patientName),
                        ("Fecha", dateLabel),
                        ("Hora", appointment.StartTime.ToString()),
                    ]
                )
            );
        }
        else
        {
            // Notify therapist
            await notificationRepository.AddAsync(new Notification
            {
                UserId = therapist.UserId,
                Title = "Cita cancelada",
                Message = $"{parentName} cancelÃ³ la cita del {dateLabel} a las {appointment.StartTime} con {patientName}.",
                AppointmentId = appointment.Id,
            });
            await notificationRepository.SaveChangesAsync();

            await emailService.SendAsync(
                therapist.User.Email, therapistName,
                "Una cita ha sido cancelada",
                BuildEmailHtml(
                    $"Hola {therapist.User.FirstName},",
                    "Un padre ha cancelado una cita.",
                    [
                        ("Paciente", patientName),
                        ("Padre/Madre", parentName),
                        ("Fecha", dateLabel),
                        ("Hora", appointment.StartTime.ToString()),
                    ]
                )
            );
        }

        return MapToAppointmentResponse(appointment);
    }

    public async Task<AppointmentResponse> CompleteAppointmentAsync(Guid appointmentId, Guid requestingUserId)
    {
        var appointment = await appointmentRepository.GetByIdAsync(appointmentId)
            ?? throw new KeyNotFoundException("Appointment not found.");

        var therapist = appointment.Therapist;
        if (therapist.UserId != requestingUserId)
            throw new UnauthorizedAccessException("Only the assigned therapist can complete this appointment.");

        if (appointment.Status != AppointmentStatus.Confirmed && appointment.Status != AppointmentStatus.InProgress)
            throw new InvalidOperationException("Only confirmed or in-progress appointments can be completed.");

        appointment.Status = AppointmentStatus.Completed;
        await appointmentRepository.SaveChangesAsync();

        var dateLabel = appointment.Date.ToString("dd/MM/yyyy");
        var therapistName = $"{therapist.User.FirstName} {therapist.User.LastName}";
        var patientName = $"{appointment.Patient.FirstName} {appointment.Patient.LastName}";
        var parent = appointment.Parent;
        var parentName = $"{parent.User.FirstName} {parent.User.LastName}";

        await notificationRepository.AddAsync(new Notification
        {
            UserId = parent.UserId,
            Title = "SesiÃ³n completada",
            Message = $"La sesiÃ³n de {patientName} del {dateLabel} con {therapistName} fue completada.",
            AppointmentId = appointment.Id,
        });
        await notificationRepository.SaveChangesAsync();

        await emailService.SendAsync(
            parent.User.Email, parentName,
            "SesiÃ³n completada",
            BuildEmailHtml(
                $"Hola {parent.User.FirstName},",
                "La sesiÃ³n de terapia ha sido marcada como completada.",
                [
                    ("Paciente", patientName),
                    ("Terapeuta", therapistName),
                    ("Fecha", dateLabel),
                    ("Hora", appointment.StartTime.ToString()),
                ]
            )
        );

        return MapToAppointmentResponse(appointment);
    }

    public async Task<AppointmentResponse> MarkEnRouteAsync(Guid appointmentId, Guid therapistUserId)
    {
        var appointment = await appointmentRepository.GetByIdAsync(appointmentId)
            ?? throw new KeyNotFoundException("Appointment not found.");

        if (appointment.Therapist.UserId != therapistUserId)
            throw new UnauthorizedAccessException("Only the assigned therapist can update this appointment.");

        if (appointment.Status != AppointmentStatus.Confirmed)
            throw new InvalidOperationException("Only confirmed appointments can be marked as en route.");

        appointment.Status = AppointmentStatus.EnRoute;
        appointment.EnRouteAt = DateTime.UtcNow;
        await appointmentRepository.SaveChangesAsync();

        var dateLabel = appointment.Date.ToString("dd/MM/yyyy");
        var therapistName = $"{appointment.Therapist.User.FirstName} {appointment.Therapist.User.LastName}";
        var patientName = $"{appointment.Patient.FirstName} {appointment.Patient.LastName}";
        var parent = appointment.Parent;

        await notificationRepository.AddAsync(new Notification
        {
            UserId = parent.UserId,
            Title = "El terapeuta estÃ¡ en camino",
            Message = $"{therapistName} estÃ¡ en camino para la sesiÃ³n de {patientName} del {dateLabel} a las {appointment.StartTime}.",
            AppointmentId = appointment.Id,
        });
        await notificationRepository.SaveChangesAsync();

        return MapToAppointmentResponse(appointment);
    }

    public async Task<AppointmentResponse> MarkInProgressAsync(Guid appointmentId, Guid therapistUserId)
    {
        var appointment = await appointmentRepository.GetByIdAsync(appointmentId)
            ?? throw new KeyNotFoundException("Appointment not found.");

        if (appointment.Therapist.UserId != therapistUserId)
            throw new UnauthorizedAccessException("Only the assigned therapist can update this appointment.");

        if (appointment.Status != AppointmentStatus.EnRoute)
            throw new InvalidOperationException("Appointment must be en route before starting.");

        appointment.Status = AppointmentStatus.InProgress;
        appointment.InProgressAt = DateTime.UtcNow;
        await appointmentRepository.SaveChangesAsync();

        var dateLabel = appointment.Date.ToString("dd/MM/yyyy");
        var therapistName = $"{appointment.Therapist.User.FirstName} {appointment.Therapist.User.LastName}";
        var patientName = $"{appointment.Patient.FirstName} {appointment.Patient.LastName}";
        var parent = appointment.Parent;

        await notificationRepository.AddAsync(new Notification
        {
            UserId = parent.UserId,
            Title = "Â¡La sesiÃ³n ha comenzado!",
            Message = $"{therapistName} iniciÃ³ la sesiÃ³n de {patientName} del {dateLabel}.",
            AppointmentId = appointment.Id,
        });
        await notificationRepository.SaveChangesAsync();

        return MapToAppointmentResponse(appointment);
    }

    // â"€â"€ Patients â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€

    public async Task<PatientResponse> AddPatientAsync(Guid parentUserId, PatientRequest request)
    {
        var parent = await parentRepository.GetByUserIdAsync(parentUserId)
            ?? throw new KeyNotFoundException("Parent not found.");

        var patient = new Patient
        {
            ParentId = parent.Id,
            FirstName = request.FirstName,
            LastName = request.LastName,
            DateOfBirth = request.DateOfBirth,
            Notes = request.Notes
        };

        await patientRepository.AddAsync(patient);
        await patientRepository.SaveChangesAsync();

        return new PatientResponse
        {
            Id = patient.Id,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            DateOfBirth = patient.DateOfBirth,
            Notes = patient.Notes,
            ParentName = $"{parent.User.FirstName} {parent.User.LastName}"
        };
    }

    public async Task<List<PatientResponse>> GetPatientsByParentAsync(Guid parentUserId)
    {
        var parent = await parentRepository.GetByUserIdAsync(parentUserId)
            ?? throw new KeyNotFoundException("Parent not found.");

        var patients = await patientRepository.GetByParentAsync(parent.Id);
        return patients.Select(p => new PatientResponse
        {
            Id = p.Id,
            FirstName = p.FirstName,
            LastName = p.LastName,
            DateOfBirth = p.DateOfBirth,
            Notes = p.Notes,
            ParentName = $"{parent.User.FirstName} {parent.User.LastName}"
        }).ToList();
    }

    public async Task<List<PatientResponse>> GetAllPatientsAsync()
    {
        var patients = await patientRepository.GetAllAsync();
        return patients.Select(p => new PatientResponse
        {
            Id = p.Id,
            FirstName = p.FirstName,
            LastName = p.LastName,
            DateOfBirth = p.DateOfBirth,
            Notes = p.Notes,
            ParentName = p.Parent?.User != null ? $"{p.Parent.User.FirstName} {p.Parent.User.LastName}" : ""
        }).ToList();
    }

    // â"€â"€ Progress & Ratings â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€

    public async Task<AppointmentResponse> AddProgressNotesAsync(Guid appointmentId, Guid therapistUserId, ProgressRequest request)
    {
        var appointment = await appointmentRepository.GetByIdAsync(appointmentId)
            ?? throw new KeyNotFoundException("Appointment not found.");

        if (appointment.Therapist.UserId != therapistUserId)
            throw new UnauthorizedAccessException("Only the assigned therapist can add progress notes.");

        if (appointment.Status != AppointmentStatus.Completed)
            throw new InvalidOperationException("Progress notes can only be added to completed appointments.");

        appointment.ProgressNotes = request.ProgressNotes;
        appointment.ProgressObjectives = request.Objectives;
        appointment.ProgressBehavior = request.Behavior;
        appointment.ProgressCommunication = request.Communication;
        appointment.ProgressSocialInteraction = request.SocialInteraction;
        appointment.ProgressSensoryResponse = request.SensoryResponse;
        appointment.ProgressAchievements = request.Achievements;
        appointment.ProgressAreasToReinforce = request.AreasToReinforce;
        appointment.ProgressRecommendations = request.Recommendations;
        appointment.ProgressParticipationLevel = request.ParticipationLevel;
        appointment.ProgressUpdatedAt = DateTime.UtcNow;
        await appointmentRepository.SaveChangesAsync();

        return MapToAppointmentResponse(appointment);
    }

    public async Task<RatingResponse> RateTherapistAsync(Guid appointmentId, Guid parentUserId, RatingRequest request)
    {
        if (request.Stars < 1 || request.Stars > 5)
            throw new ArgumentException("Stars must be between 1 and 5.");

        var appointment = await appointmentRepository.GetByIdAsync(appointmentId)
            ?? throw new KeyNotFoundException("Appointment not found.");

        if (appointment.Parent.UserId != parentUserId)
            throw new UnauthorizedAccessException("Only the parent who booked this appointment can rate it.");

        if (appointment.Status != AppointmentStatus.Completed)
            throw new InvalidOperationException("Only completed appointments can be rated.");

        var existing = await ratingRepository.GetByAppointmentAsync(appointmentId);
        if (existing is not null)
            throw new InvalidOperationException("This appointment has already been rated.");

        var rating = new TherapistRating
        {
            AppointmentId = appointment.Id,
            TherapistId = appointment.TherapistId,
            ParentId = appointment.ParentId,
            Stars = request.Stars,
            Comment = request.Comment
        };

        await ratingRepository.AddAsync(rating);
        await ratingRepository.SaveChangesAsync();

        var therapist = appointment.Therapist;
        var parentForRating = appointment.Parent;

        await notificationRepository.AddAsync(new Notification
        {
            UserId = therapist.UserId,
            Title = "Nueva calificaciÃ³n recibida",
            Message = $"{parentForRating.User.FirstName} {parentForRating.User.LastName} calificÃ³ tu sesiÃ³n con {request.Stars} estrella(s).",
            AppointmentId = appointment.Id,
        });
        await notificationRepository.SaveChangesAsync();

        return new RatingResponse
        {
            Id = rating.Id,
            AppointmentId = rating.AppointmentId,
            TherapistId = therapist.Id,
            TherapistName = $"{therapist.User.FirstName} {therapist.User.LastName}",
            ParentId = appointment.ParentId,
            ParentName = $"{parentForRating.User.FirstName} {parentForRating.User.LastName}",
            Stars = rating.Stars,
            Comment = rating.Comment,
            CreatedAt = rating.CreatedAt
        };
    }

    public async Task<List<RatingResponse>> GetTherapistRatingsAsync(Guid therapistId)
    {
        var therapist = await therapistRepository.GetByIdAsync(therapistId)
            ?? throw new KeyNotFoundException("Therapist not found.");

        var ratings = await ratingRepository.GetByTherapistAsync(therapistId);
        return ratings.Select(r => new RatingResponse
        {
            Id = r.Id,
            AppointmentId = r.AppointmentId,
            TherapistId = therapist.Id,
            TherapistName = $"{therapist.User.FirstName} {therapist.User.LastName}",
            ParentId = r.ParentId,
            ParentName = $"{r.Parent.User.FirstName} {r.Parent.User.LastName}",
            Stars = r.Stars,
            Comment = r.Comment,
            CreatedAt = r.CreatedAt
        }).ToList();
    }

    public async Task<List<RatingResponse>> GetMyRatingsAsync(Guid therapistUserId)
    {
        var therapist = await therapistRepository.GetByUserIdAsync(therapistUserId)
            ?? throw new KeyNotFoundException("Therapist not found.");

        return await GetTherapistRatingsAsync(therapist.Id);
    }

    // â"€â"€ Admin â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€

    public async Task<AppointmentResponse> ConfirmAppointmentAsync(Guid appointmentId)
    {
        var appointment = await appointmentRepository.GetByIdAsync(appointmentId)
            ?? throw new KeyNotFoundException("Appointment not found.");

        if (appointment.Status != AppointmentStatus.Pending)
            throw new InvalidOperationException("Only pending appointments can be confirmed.");

        appointment.Status = AppointmentStatus.Confirmed;
        await appointmentRepository.SaveChangesAsync();

        var dateLabel = appointment.Date.ToString("dd/MM/yyyy");
        var therapistName = $"{appointment.Therapist.User.FirstName} {appointment.Therapist.User.LastName}";
        var patientName = $"{appointment.Patient.FirstName} {appointment.Patient.LastName}";
        var parent = appointment.Parent;
        var parentName = $"{parent.User.FirstName} {parent.User.LastName}";

        // Notify parent
        await notificationRepository.AddAsync(new Notification
        {
            UserId = parent.UserId,
            Title = "Cita confirmada ",
            Message = $"Tu cita para {patientName} con {therapistName} el {dateLabel} a las {appointment.StartTime} fue confirmada.",
            AppointmentId = appointment.Id,
        });

        // Notify therapist
        await notificationRepository.AddAsync(new Notification
        {
            UserId = appointment.Therapist.UserId,
            Title = "Nueva cita confirmada",
            Message = $"Tienes una nueva cita con {patientName} el {dateLabel} a las {appointment.StartTime}.",
            AppointmentId = appointment.Id,
        });

        await notificationRepository.SaveChangesAsync();

        await emailService.SendAsync(
            parent.User.Email, parentName,
            "Â¡Tu cita fue confirmada!",
            BuildEmailHtml(
                $"Hola {parent.User.FirstName},",
                "Â¡Buenas noticias! Tu cita ha sido confirmada.",
                [
                    ("Paciente", patientName),
                    ("Terapeuta", therapistName),
                    ("Fecha", dateLabel),
                    ("Hora", appointment.StartTime.ToString()),
                ]
            )
        );

        await emailService.SendAsync(
            appointment.Therapist.User.Email, therapistName,
            "Nueva cita confirmada en tu agenda",
            BuildEmailHtml(
                $"Hola {appointment.Therapist.User.FirstName},",
                "Se ha confirmado una nueva cita en tu agenda.",
                [
                    ("Paciente", patientName),
                    ("Padre/Madre", parentName),
                    ("Fecha", dateLabel),
                    ("Hora", appointment.StartTime.ToString()),
                ]
            )
        );

        return MapToAppointmentResponse(appointment);
    }

    public async Task<AppointmentResponse> RejectAppointmentAsync(Guid appointmentId)
    {
        var appointment = await appointmentRepository.GetByIdAsync(appointmentId)
            ?? throw new KeyNotFoundException("Appointment not found.");

        if (appointment.Status != AppointmentStatus.Pending)
            throw new InvalidOperationException("Only pending appointments can be rejected.");

        appointment.Status = AppointmentStatus.Cancelled;
        await appointmentRepository.SaveChangesAsync();

        var dateLabel = appointment.Date.ToString("dd/MM/yyyy");
        var therapistName = $"{appointment.Therapist.User.FirstName} {appointment.Therapist.User.LastName}";
        var patientName = $"{appointment.Patient.FirstName} {appointment.Patient.LastName}";
        var parent = appointment.Parent;
        var parentName = $"{parent.User.FirstName} {parent.User.LastName}";

        await notificationRepository.AddAsync(new Notification
        {
            UserId = parent.UserId,
            Title = "Cita no aprobada",
            Message = $"Tu solicitud de cita para {patientName} con {therapistName} el {dateLabel} no fue aprobada.",
            AppointmentId = appointment.Id,
        });
        await notificationRepository.SaveChangesAsync();

        await emailService.SendAsync(
            parent.User.Email, parentName,
            "Tu solicitud de cita no fue aprobada",
            BuildEmailHtml(
                $"Hola {parent.User.FirstName},",
                "Lamentamos informarte que tu solicitud de cita no fue aprobada en esta ocasiÃ³n.",
                [
                    ("Paciente", patientName),
                    ("Terapeuta", therapistName),
                    ("Fecha solicitada", dateLabel),
                    ("Hora solicitada", appointment.StartTime.ToString()),
                ]
            )
        );

        return MapToAppointmentResponse(appointment);
    }

    public async Task<List<AppointmentResponse>> GetPendingAppointmentsAsync()
    {
        var appointments = await appointmentRepository.GetAllAsync();
        return appointments
            .Where(a => a.Status == AppointmentStatus.Pending)
            .Select(MapToAppointmentResponse).ToList();
    }

    public async Task<List<AppointmentResponse>> GetAllAppointmentsAsync()
    {
        var appointments = await appointmentRepository.GetAllAsync();
        return appointments.Select(MapToAppointmentResponse).ToList();
    }

    // â"€â"€ Helpers â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€

    private static List<TimeSlotResponse> BuildTimeSlots(TherapistAvailability availability)
    {
        var slots = new List<TimeSlotResponse>();
        var bookedStartTimes = availability.Appointments
            .Where(a => a.Status != AppointmentStatus.Cancelled)
            .Select(a => a.StartTime)
            .ToHashSet();

        var current = availability.StartTime;
        while (current.AddMinutes(availability.SessionDurationMinutes) <= availability.EndTime)
        {
            var end = current.AddMinutes(availability.SessionDurationMinutes);
            slots.Add(new TimeSlotResponse
            {
                StartTime = current,
                EndTime = end,
                IsAvailable = !bookedStartTimes.Contains(current)
            });
            current = end;
        }

        return slots;
    }

    private static AvailabilityResponse MapToAvailabilityResponse(
        TherapistAvailability availability,
        Therapist therapist,
        List<TimeSlotResponse> slots) => new()
    {
        Id = availability.Id,
        TherapistId = therapist.Id,
        TherapistName = $"{therapist.User.FirstName} {therapist.User.LastName}",
        Date = availability.Date,
        StartTime = availability.StartTime,
        EndTime = availability.EndTime,
        SessionDurationMinutes = availability.SessionDurationMinutes,
        AvailableSlots = slots
    };

    private static AppointmentResponse MapToAppointmentResponse(Appointment a) => new()
    {
        Id = a.Id,
        TherapistId = a.TherapistId,
        TherapistName = $"{a.Therapist.User.FirstName} {a.Therapist.User.LastName}",
        PatientId = a.PatientId,
        PatientName = $"{a.Patient.FirstName} {a.Patient.LastName}",
        Date = a.Date,
        StartTime = a.StartTime,
        EndTime = a.EndTime,
        Status = a.Status,
        Notes = a.Notes,
        ProgressNotes = a.ProgressNotes,
        ProgressObjectives = a.ProgressObjectives,
        ProgressBehavior = a.ProgressBehavior,
        ProgressCommunication = a.ProgressCommunication,
        ProgressSocialInteraction = a.ProgressSocialInteraction,
        ProgressSensoryResponse = a.ProgressSensoryResponse,
        ProgressAchievements = a.ProgressAchievements,
        ProgressAreasToReinforce = a.ProgressAreasToReinforce,
        ProgressRecommendations = a.ProgressRecommendations,
        ProgressParticipationLevel = a.ProgressParticipationLevel,
        ProgressUpdatedAt = a.ProgressUpdatedAt,
        EnRouteAt = a.EnRouteAt,
        InProgressAt = a.InProgressAt,
        IsRated = a.Rating != null,
        CreatedAt = a.CreatedAt
    };

    private static string BuildEmailHtml(string greeting, string intro, (string label, string value)[] details)
    {
        var rows = string.Concat(details.Select(d =>
            $"<tr><td style='padding:6px 12px;color:#6b7280;font-size:14px;'>{d.label}</td>" +
            $"<td style='padding:6px 12px;font-weight:600;font-size:14px;'>{d.value}</td></tr>"));

        return $"""
            <!DOCTYPE html>
            <html>
            <body style="font-family:sans-serif;background:#f9fafb;margin:0;padding:24px;">
              <div style="max-width:480px;margin:0 auto;background:#fff;border-radius:12px;padding:32px;box-shadow:0 1px 4px rgba(0,0,0,.08);">
                <h2 style="color:#4f46e5;margin-top:0;">ðŸ¥œ Cacahuate</h2>
                <p style="color:#111827;font-size:16px;">{greeting}</p>
                <p style="color:#374151;font-size:15px;">{intro}</p>
                <table style="width:100%;border-collapse:collapse;margin-top:16px;background:#f3f4f6;border-radius:8px;">
                  {rows}
                </table>
                <p style="color:#9ca3af;font-size:12px;margin-top:24px;">Este es un mensaje automÃ¡tico de Cacahuate. No respondas a este correo.</p>
              </div>
            </body>
            </html>
            """;
    }
}

