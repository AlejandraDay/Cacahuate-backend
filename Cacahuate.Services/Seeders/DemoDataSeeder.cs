using System.Text.Json;
using Cacahuate.DataAccess.Context;
using Cacahuate.DataAccess.Entities;
using Cacahuate.Services.Interfaces;
using Cacahuate.Shared.DTOs.Auth;
using Cacahuate.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Cacahuate.Services.Seeders;

/// <summary>
/// Seeds a full demo dataset (therapists, parents, patients, availabilities,
/// appointments in every status, ratings, form assignments/submissions) so a
/// stakeholder can log in to any portal and see the app fully populated.
///
/// Reset-and-reseed: on every Development startup, checks whether all 7 demo
/// accounts (therapist2-4, parent2-5) already exist. If so, does nothing at
/// all. Otherwise (fresh DB, partial/failed prior run, or unrelated manual
/// data) it wipes every seed-related domain row - except the 3 baseline
/// accounts created by the SeedInitialUsers migration (admin1, therapist1,
/// parent1) and their directly-owned rows - and reseeds from scratch.
/// FormTemplate/FormField rows (owned by FormTemplateSeeder) are never touched.
/// </summary>
public static class DemoDataSeeder
{
    private const string DemoPassword = "Control123";

    private const string BaselineAdminEmail = "admin1@gmail.com";
    private const string BaselineTherapistEmail = "therapist1@gmail.com";
    private const string BaselineParentEmail = "parent1@gmail.com";

    private static readonly string[] DemoAccountEmails =
    {
        "therapist2@gmail.com", "therapist3@gmail.com", "therapist4@gmail.com",
        "parent2@gmail.com", "parent3@gmail.com", "parent4@gmail.com", "parent5@gmail.com",
    };

    public static async Task SeedAsync(AppDbContext db, IAuthService authService)
    {
        var existingDemoAccounts = await db.Users
            .CountAsync(u => DemoAccountEmails.Contains(u.Email));
        if (existingDemoAccounts == DemoAccountEmails.Length) return;

        await ResetDemoDataAsync(db);

        var rnd = new Random(42);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Npgsql requires DateTimeKind.Utc for "timestamp with time zone" columns;
        // DateOnly.ToDateTime(TimeOnly) produces Kind=Unspecified, so every
        // date+time combination must be routed through this helper.
        static DateTime ToUtc(DateOnly date, TimeOnly time) =>
            DateTime.SpecifyKind(date.ToDateTime(time), DateTimeKind.Utc);

        // ── 1. Additional therapists ─────────────────────────────────────
        var newTherapistSpecs = new[]
        {
            ("Laura", "Martínez", "therapist2@gmail.com", "Especialista en terapia ocupacional infantil con 8 años de experiencia."),
            ("Carlos", "Sánchez", "therapist3@gmail.com", "Terapeuta del lenguaje enfocado en comunicación aumentativa y alternativa."),
            ("Marta", "Gómez", "therapist4@gmail.com", "Especialista en análisis conductual aplicado (ABA) para niños con TEA."),
        };

        foreach (var (firstName, lastName, email, bio) in newTherapistSpecs)
        {
            var created = await authService.CreateTherapistAsync(new RegisterRequest
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Password = DemoPassword,
                Role = UserRole.Therapist
            });

            var therapist = await db.Therapists.FirstAsync(t => t.UserId == created.Id);
            therapist.Bio = bio;
        }
        await db.SaveChangesAsync();

        // ── 2. Additional parents ────────────────────────────────────────
        var newParentSpecs = new[]
        {
            ("Ana", "Torres", "parent2@gmail.com"),
            ("Luis", "Ramírez", "parent3@gmail.com"),
            ("Sofía", "Herrera", "parent4@gmail.com"),
            ("Diego", "Morales", "parent5@gmail.com"),
        };

        foreach (var (firstName, lastName, email) in newParentSpecs)
        {
            await authService.CreateParentAsync(new RegisterRequest
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Password = DemoPassword,
                Role = UserRole.Parent
            });
        }
        await db.SaveChangesAsync();

        // ── 3. Look up the pre-existing seeded users (from migration) ───
        var admin = await db.Users.FirstAsync(u => u.Email == BaselineAdminEmail);
        var therapist1 = await db.Therapists.FirstAsync(t => t.User.Email == BaselineTherapistEmail);
        var parent1 = await db.Parents.FirstAsync(p => p.User.Email == BaselineParentEmail);

        var therapist2 = await db.Therapists.FirstAsync(t => t.User.Email == "therapist2@gmail.com");
        var therapist3 = await db.Therapists.FirstAsync(t => t.User.Email == "therapist3@gmail.com");
        var therapist4 = await db.Therapists.FirstAsync(t => t.User.Email == "therapist4@gmail.com");

        var parent2 = await db.Parents.FirstAsync(p => p.User.Email == "parent2@gmail.com");
        var parent3 = await db.Parents.FirstAsync(p => p.User.Email == "parent3@gmail.com");
        var parent4 = await db.Parents.FirstAsync(p => p.User.Email == "parent4@gmail.com");
        var parent5 = await db.Parents.FirstAsync(p => p.User.Email == "parent5@gmail.com");

        // ── 4. Patients ───────────────────────────────────────────────────
        Patient MakePatient(Guid parentId, string first, string last, int ageYears, string? notes = null) => new()
        {
            ParentId = parentId,
            FirstName = first,
            LastName = last,
            DateOfBirth = today.AddYears(-ageYears).AddDays(-rnd.Next(0, 300)),
            Notes = notes
        };

        var mateoPerez = MakePatient(parent1.Id, "Mateo", "Pérez", 6, "Diagnóstico: TEA nivel 1.");
        var valentinaPerez = MakePatient(parent1.Id, "Valentina", "Pérez", 9);
        var emmaTorres = MakePatient(parent2.Id, "Emma", "Torres", 5);
        var santiagoRamirez = MakePatient(parent3.Id, "Santiago", "Ramírez", 8, "Requiere apoyo en comunicación.");
        var isabellaHerrera = MakePatient(parent4.Id, "Isabella", "Herrera", 4);
        var mateoMorales = MakePatient(parent5.Id, "Mateo", "Morales", 10);
        var luciaMorales = MakePatient(parent5.Id, "Lucía", "Morales", 7, "Diagnóstico: TDAH.");

        db.Patients.AddRange(mateoPerez, valentinaPerez, emmaTorres, santiagoRamirez, isabellaHerrera, mateoMorales, luciaMorales);
        await db.SaveChangesAsync();

        var allPatients = new[] { mateoPerez, valentinaPerez, emmaTorres, santiagoRamirez, isabellaHerrera, mateoMorales, luciaMorales };

        // ── 5. Availabilities ────────────────────────────────────────────
        var therapistIds = new[] { therapist1.Id, therapist2.Id, therapist3.Id, therapist4.Id };
        var dates = new[]
        {
            today.AddDays(-14), today.AddDays(-10), today.AddDays(-7), today.AddDays(-3),
            today,
            today.AddDays(3), today.AddDays(7), today.AddDays(10), today.AddDays(14),
        };

        var morningStart = new TimeOnly(9, 0);
        var morningEnd = new TimeOnly(12, 0);
        var afternoonStart = new TimeOnly(14, 0);
        var afternoonEnd = new TimeOnly(17, 0);

        var availabilityLookup = new Dictionary<(Guid TherapistId, DateOnly Date, bool Morning), TherapistAvailability>();
        var availabilities = new List<TherapistAvailability>();

        foreach (var tId in therapistIds)
        {
            foreach (var date in dates)
            {
                var morning = new TherapistAvailability
                {
                    TherapistId = tId,
                    Date = date,
                    StartTime = morningStart,
                    EndTime = morningEnd,
                    SessionDurationMinutes = 60
                };
                var afternoon = new TherapistAvailability
                {
                    TherapistId = tId,
                    Date = date,
                    StartTime = afternoonStart,
                    EndTime = afternoonEnd,
                    SessionDurationMinutes = 60
                };
                availabilities.Add(morning);
                availabilities.Add(afternoon);
                availabilityLookup[(tId, date, true)] = morning;
                availabilityLookup[(tId, date, false)] = afternoon;
            }
        }

        db.TherapistAvailabilities.AddRange(availabilities);
        await db.SaveChangesAsync();

        // ── 6. Appointments across all statuses ─────────────────────────
        var past1 = today.AddDays(-14);
        var past2 = today.AddDays(-10);
        var past3 = today.AddDays(-7);
        var past4 = today.AddDays(-3);
        var future1 = today.AddDays(3);
        var future2 = today.AddDays(7);
        var future3 = today.AddDays(10);
        var future4 = today.AddDays(14);

        Appointment MakeAppointment(
            Therapist therapist, Patient patient, Parent parent,
            DateOnly date, TimeOnly start, TimeOnly end, bool morningBlock,
            AppointmentStatus status)
        {
            var availability = availabilityLookup[(therapist.Id, date, morningBlock)];
            return new Appointment
            {
                TherapistId = therapist.Id,
                PatientId = patient.Id,
                ParentId = parent.Id,
                AvailabilityId = availability.Id,
                Date = date,
                StartTime = start,
                EndTime = end,
                Status = status
            };
        }

        // Completed (past) appointments
        var completed1 = MakeAppointment(therapist1, mateoPerez, parent1, past1, new TimeOnly(9, 0), new TimeOnly(10, 0), true, AppointmentStatus.Completed);
        var completed2 = MakeAppointment(therapist1, valentinaPerez, parent1, past1, new TimeOnly(10, 0), new TimeOnly(11, 0), true, AppointmentStatus.Completed);
        var completed3 = MakeAppointment(therapist2, emmaTorres, parent2, past2, new TimeOnly(9, 0), new TimeOnly(10, 0), true, AppointmentStatus.Completed);
        var completed4 = MakeAppointment(therapist2, santiagoRamirez, parent3, past2, new TimeOnly(10, 0), new TimeOnly(11, 0), true, AppointmentStatus.Completed);
        var completed5 = MakeAppointment(therapist3, isabellaHerrera, parent4, past3, new TimeOnly(9, 0), new TimeOnly(10, 0), true, AppointmentStatus.Completed);
        var completed6 = MakeAppointment(therapist3, mateoMorales, parent5, past3, new TimeOnly(10, 0), new TimeOnly(11, 0), true, AppointmentStatus.Completed);
        var completed7 = MakeAppointment(therapist4, luciaMorales, parent5, past4, new TimeOnly(9, 0), new TimeOnly(10, 0), true, AppointmentStatus.Completed);
        var completedAppointments = new[] { completed1, completed2, completed3, completed4, completed5, completed6, completed7 };

        var progressLevels = new[] { ParticipationLevel.High, ParticipationLevel.Medium, ParticipationLevel.Low, ParticipationLevel.High, ParticipationLevel.Medium, ParticipationLevel.High, ParticipationLevel.Low };
        for (int i = 0; i < completedAppointments.Length; i++)
        {
            var appt = completedAppointments[i];
            var apptDateTime = ToUtc(appt.Date, appt.EndTime);
            appt.ProgressNotes = "El paciente participó activamente durante toda la sesión, mostrando buena disposición a las actividades propuestas.";
            appt.ProgressObjectives = "Reforzar habilidades de comunicación funcional y seguimiento de instrucciones simples.";
            appt.ProgressBehavior = "Se observó un comportamiento colaborador, con momentos puntuales de frustración manejados adecuadamente.";
            appt.ProgressCommunication = "Utilizó frases cortas para expresar necesidades y respondió a preguntas directas.";
            appt.ProgressSocialInteraction = "Interactuó positivamente con el terapeuta, iniciando contacto visual y turnos de juego.";
            appt.ProgressSensoryResponse = "Respuesta sensorial dentro de lo esperado, sin aversiones significativas durante la sesión.";
            appt.ProgressAchievements = "Logró completar la actividad principal con apoyo mínimo del terapeuta.";
            appt.ProgressAreasToReinforce = "Continuar trabajando la tolerancia a la frustración y la transición entre actividades.";
            appt.ProgressRecommendations = "Mantener la frecuencia de sesiones semanales y reforzar en casa las rutinas trabajadas.";
            appt.ProgressParticipationLevel = progressLevels[i];
            appt.ProgressUpdatedAt = apptDateTime;
            appt.ParentSignature = "Firmado digitalmente por el padre/madre de familia";
            appt.ParentSignedAt = apptDateTime.AddMinutes(5);
            appt.TherapistSignature = "Firmado digitalmente por el terapeuta";
            appt.TherapistSignedAt = apptDateTime.AddMinutes(10);
        }

        // Cancelled
        var cancelled1 = MakeAppointment(therapist1, valentinaPerez, parent1, past4, new TimeOnly(14, 0), new TimeOnly(15, 0), false, AppointmentStatus.Cancelled);
        cancelled1.Notes = "Cancelada por el padre de familia debido a una emergencia.";
        var cancelled2 = MakeAppointment(therapist2, emmaTorres, parent2, future1, new TimeOnly(9, 0), new TimeOnly(10, 0), true, AppointmentStatus.Cancelled);
        cancelled2.Notes = "Cancelada por reprogramación del terapeuta.";

        // Pending (future)
        var pending1 = MakeAppointment(therapist1, mateoPerez, parent1, future1, new TimeOnly(10, 0), new TimeOnly(11, 0), true, AppointmentStatus.Pending);
        var pending2 = MakeAppointment(therapist2, santiagoRamirez, parent3, future2, new TimeOnly(9, 0), new TimeOnly(10, 0), true, AppointmentStatus.Pending);
        var pending3 = MakeAppointment(therapist3, isabellaHerrera, parent4, future2, new TimeOnly(10, 0), new TimeOnly(11, 0), true, AppointmentStatus.Pending);
        var pending4 = MakeAppointment(therapist4, mateoMorales, parent5, future3, new TimeOnly(9, 0), new TimeOnly(10, 0), true, AppointmentStatus.Pending);

        // Confirmed (future)
        var confirmed1 = MakeAppointment(therapist1, valentinaPerez, parent1, future3, new TimeOnly(10, 0), new TimeOnly(11, 0), true, AppointmentStatus.Confirmed);
        var confirmed2 = MakeAppointment(therapist2, emmaTorres, parent2, future4, new TimeOnly(9, 0), new TimeOnly(10, 0), true, AppointmentStatus.Confirmed);
        var confirmed3 = MakeAppointment(therapist3, luciaMorales, parent5, future4, new TimeOnly(10, 0), new TimeOnly(11, 0), true, AppointmentStatus.Confirmed);
        var confirmed4 = MakeAppointment(therapist4, santiagoRamirez, parent3, future1, new TimeOnly(14, 0), new TimeOnly(15, 0), false, AppointmentStatus.Confirmed);

        // EnRoute / InProgress — dated today, for live demoing
        var enRoute = MakeAppointment(therapist1, mateoPerez, parent1, today, new TimeOnly(9, 0), new TimeOnly(10, 0), true, AppointmentStatus.EnRoute);
        enRoute.EnRouteAt = DateTime.UtcNow.AddMinutes(-15);

        var inProgress = MakeAppointment(therapist2, emmaTorres, parent2, today, new TimeOnly(10, 0), new TimeOnly(11, 0), true, AppointmentStatus.InProgress);
        inProgress.InProgressAt = DateTime.UtcNow.AddMinutes(-10);

        db.Appointments.AddRange(completedAppointments);
        db.Appointments.AddRange(cancelled1, cancelled2, pending1, pending2, pending3, pending4,
            confirmed1, confirmed2, confirmed3, confirmed4, enRoute, inProgress);
        await db.SaveChangesAsync();

        // ── 7. Ratings for roughly half of the completed appointments ───
        var ratingComments = new[]
        {
            "Excelente terapeuta, mi hijo mejoró mucho en sus sesiones.",
            "Muy profesional y paciente con los niños.",
            "Buenos resultados, aunque a veces las sesiones se sienten cortas.",
            "Muy contentos con el progreso, lo recomendamos ampliamente.",
        };

        var ratedAppointments = new[] { completed1, completed2, completed4, completed6 };
        var stars = new[] { 5, 4, 5, 3 };
        for (int i = 0; i < ratedAppointments.Length; i++)
        {
            var appt = ratedAppointments[i];
            db.TherapistRatings.Add(new TherapistRating
            {
                AppointmentId = appt.Id,
                TherapistId = appt.TherapistId,
                ParentId = appt.ParentId,
                Stars = stars[i],
                Comment = ratingComments[i],
                CreatedAt = ToUtc(appt.Date, appt.EndTime).AddHours(1)
            });
        }
        await db.SaveChangesAsync();

        // ── 8. Form assignments ──────────────────────────────────────────
        var templates = await db.FormTemplates.Include(t => t.Fields).ToListAsync();
        var basicTemplate = templates.First(t => t.Name.Contains("Básica"));
        var intermediateTemplate = templates.First(t => t.Name.Contains("Intermedio"));
        var fullTemplate = templates.First(t => t.Name.Contains("Completo"));

        FormAssignment Assign(FormTemplate template, Patient patient, DateTime assignedAt, string? notes = null) => new()
        {
            FormTemplateId = template.Id,
            PatientId = patient.Id,
            AssignedByAdminUserId = admin.Id,
            AssignedAt = assignedAt,
            Notes = notes
        };

        var assignmentBasicMateoPerez = Assign(basicTemplate, mateoPerez, DateTime.UtcNow.AddDays(-20), "Completar antes de la evaluación inicial.");
        var assignmentBasicValentinaPerez = Assign(basicTemplate, valentinaPerez, DateTime.UtcNow.AddDays(-20));
        var assignmentBasicEmmaTorres = Assign(basicTemplate, emmaTorres, DateTime.UtcNow.AddDays(-16));
        var assignmentBasicSantiagoRamirez = Assign(basicTemplate, santiagoRamirez, DateTime.UtcNow.AddDays(-16), "Prioridad alta, primer ingreso.");
        var assignmentBasicIsabellaHerrera = Assign(basicTemplate, isabellaHerrera, DateTime.UtcNow.AddDays(-12));
        var assignmentBasicMateoMorales = Assign(basicTemplate, mateoMorales, DateTime.UtcNow.AddDays(-12));
        var assignmentBasicLuciaMorales = Assign(basicTemplate, luciaMorales, DateTime.UtcNow.AddDays(-8));

        var assignmentInterMateoPerez = Assign(intermediateTemplate, mateoPerez, DateTime.UtcNow.AddDays(-13));
        var assignmentInterValentinaPerez = Assign(intermediateTemplate, valentinaPerez, DateTime.UtcNow.AddDays(-13));
        var assignmentInterEmmaTorres = Assign(intermediateTemplate, emmaTorres, DateTime.UtcNow.AddDays(-9), "Seguimiento mensual.");
        var assignmentInterSantiagoRamirez = Assign(intermediateTemplate, santiagoRamirez, DateTime.UtcNow.AddDays(-9));

        var assignmentFullMateoPerez = Assign(fullTemplate, mateoPerez, DateTime.UtcNow.AddDays(-6), "Evaluación integral trimestral.");
        var assignmentFullValentinaPerez = Assign(fullTemplate, valentinaPerez, DateTime.UtcNow.AddDays(-6));

        db.FormAssignments.AddRange(
            assignmentBasicMateoPerez, assignmentBasicValentinaPerez, assignmentBasicEmmaTorres,
            assignmentBasicSantiagoRamirez, assignmentBasicIsabellaHerrera, assignmentBasicMateoMorales,
            assignmentBasicLuciaMorales, assignmentInterMateoPerez, assignmentInterValentinaPerez,
            assignmentInterEmmaTorres, assignmentInterSantiagoRamirez, assignmentFullMateoPerez,
            assignmentFullValentinaPerez);
        await db.SaveChangesAsync();

        // ── 9. Form submissions + answers for a subset of completed appointments ─
        string GenerateAnswerValue(FormField field)
        {
            switch (field.Type)
            {
                case FieldType.Text:
                    return "Información registrada durante la sesión.";
                case FieldType.Textarea:
                    return "El paciente mostró buena disposición y avances notables durante la actividad realizada.";
                case FieldType.Number:
                    return rnd.Next(1, 60).ToString();
                case FieldType.Select:
                    var options = string.IsNullOrWhiteSpace(field.Options)
                        ? []
                        : JsonSerializer.Deserialize<string[]>(field.Options) ?? [];
                    return options.Length > 0 ? options[rnd.Next(options.Length)] : string.Empty;
                case FieldType.Checkbox:
                    return rnd.Next(2) == 0 ? "true" : "false";
                case FieldType.Scale:
                    return rnd.Next(5, 11).ToString();
                default:
                    return string.Empty;
            }
        }

        void SubmitForm(FormAssignment assignment, Appointment appointment, FormTemplate template)
        {
            var submission = new FormSubmission
            {
                FormAssignmentId = assignment.Id,
                AppointmentId = appointment.Id,
                TherapistId = appointment.TherapistId,
                SubmittedAt = ToUtc(appointment.Date, appointment.EndTime).AddMinutes(20)
            };

            foreach (var field in template.Fields)
            {
                submission.Answers.Add(new FormAnswer
                {
                    FormFieldId = field.Id,
                    Value = GenerateAnswerValue(field)
                });
            }

            db.FormSubmissions.Add(submission);
        }

        // Submit the basic form for 4 of the 7 patients with a completed appointment;
        // leave Emma Torres, Isabella Herrera and Lucía Morales pending (no submission).
        SubmitForm(assignmentBasicMateoPerez, completed1, basicTemplate);
        SubmitForm(assignmentBasicValentinaPerez, completed2, basicTemplate);
        SubmitForm(assignmentBasicSantiagoRamirez, completed4, basicTemplate);
        SubmitForm(assignmentBasicMateoMorales, completed6, basicTemplate);

        // Also submit the intermediate/full forms for the two patients that have them.
        SubmitForm(assignmentInterMateoPerez, completed1, intermediateTemplate);
        SubmitForm(assignmentFullValentinaPerez, completed2, fullTemplate);

        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Wipes every seed-related domain row except the 3 baseline accounts
    /// (admin1, therapist1, parent1 - created once by the SeedInitialUsers
    /// migration and never recreated here) and their directly-owned rows.
    /// FormTemplate/FormField rows are shared reference data owned by
    /// FormTemplateSeeder and are deliberately left untouched.
    /// Deletion order respects FK DeleteBehavior.Restrict constraints
    /// (children before parents), per AppDbContext's OnModelCreating.
    /// </summary>
    private static async Task ResetDemoDataAsync(AppDbContext db)
    {
        var baselineEmails = new[] { BaselineAdminEmail, BaselineTherapistEmail, BaselineParentEmail };

        await db.FormAnswers.ExecuteDeleteAsync();
        await db.FormSubmissions.ExecuteDeleteAsync();
        await db.FormAssignments.ExecuteDeleteAsync();
        await db.TherapistRatings.ExecuteDeleteAsync();
        await db.Appointments.ExecuteDeleteAsync();
        await db.TherapistAvailabilities.ExecuteDeleteAsync();
        await db.Patients.ExecuteDeleteAsync();

        await db.Therapists
            .Where(t => t.User.Email != BaselineTherapistEmail)
            .ExecuteDeleteAsync();

        await db.Parents
            .Where(p => p.User.Email != BaselineParentEmail)
            .ExecuteDeleteAsync();

        await db.Users
            .Where(u => (u.Role == UserRole.Therapist || u.Role == UserRole.Parent)
                        && !baselineEmails.Contains(u.Email))
            .ExecuteDeleteAsync();
    }
}
