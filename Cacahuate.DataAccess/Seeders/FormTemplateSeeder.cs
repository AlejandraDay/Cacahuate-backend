using Cacahuate.DataAccess.Context;
using Cacahuate.DataAccess.Entities;
using Cacahuate.Shared.Enums;
using System.Text.Json;

namespace Cacahuate.DataAccess.Seeders;

public static class FormTemplateSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (db.FormTemplates.Any()) return;

        var templates = new List<FormTemplate>
        {
            // ── Nivel 1: Básico ────────────────────────────────────────────────
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Evaluación Inicial (Básica)",
                Description = "Formulario de primer contacto con información esencial del paciente.",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Fields =
                [
                    new() { Label = "Nombre completo del paciente", Type = FieldType.Text,     IsRequired = true,  Order = 1 },
                    new() { Label = "Edad",                          Type = FieldType.Number,   IsRequired = true,  Order = 2 },
                    new() { Label = "Motivo de consulta",            Type = FieldType.Textarea, IsRequired = true,  Order = 3 },
                    new() { Label = "¿Primera vez en terapia?",      Type = FieldType.Checkbox, IsRequired = true,  Order = 4 },
                ]
            },

            // ── Nivel 2: Intermedio ────────────────────────────────────────────
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Seguimiento de Sesión (Intermedio)",
                Description = "Registro de progreso por sesión: objetivos, técnicas aplicadas y observaciones.",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Fields =
                [
                    new() { Label = "Objetivo trabajado en sesión",          Type = FieldType.Text,     IsRequired = true,  Order = 1 },
                    new() { Label = "Técnica utilizada",
                            Type = FieldType.Select,
                            Options = JsonSerializer.Serialize(new[] { "ABA", "Juego libre", "PECS", "DIR/Floortime", "Terapia cognitivo-conductual", "Otro" }),
                            IsRequired = true,  Order = 2 },
                    new() { Label = "Nivel de cooperación del paciente (1-10)", Type = FieldType.Scale,    IsRequired = true,  Order = 3 },
                    new() { Label = "¿Se alcanzó el objetivo?",              Type = FieldType.Checkbox, IsRequired = true,  Order = 4 },
                    new() { Label = "Observaciones del terapeuta",           Type = FieldType.Textarea, IsRequired = false, Order = 5 },
                    new() { Label = "Duración efectiva de la sesión (min)",  Type = FieldType.Number,   IsRequired = true,  Order = 6 },
                ]
            },

            // ── Nivel 3: Completo ──────────────────────────────────────────────
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Evaluación de Desarrollo Integral (Completo)",
                Description = "Evaluación exhaustiva de áreas de desarrollo: cognitiva, social, emocional, comunicativa y motora.",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Fields =
                [
                    // Datos generales
                    new() { Label = "Nombre completo del paciente",       Type = FieldType.Text,     IsRequired = true,  Order = 1 },
                    new() { Label = "Edad (años)",                        Type = FieldType.Number,   IsRequired = true,  Order = 2 },
                    new() { Label = "Diagnóstico principal",
                            Type = FieldType.Select,
                            Options = JsonSerializer.Serialize(new[] { "TEA nivel 1", "TEA nivel 2", "TEA nivel 3", "TDAH", "Retraso en el desarrollo", "Sin diagnóstico formal", "Otro" }),
                            IsRequired = true,  Order = 3 },

                    // Área cognitiva
                    new() { Label = "Nivel de atención sostenida (1-10)",  Type = FieldType.Scale,    IsRequired = true,  Order = 4 },
                    new() { Label = "Seguimiento de instrucciones simples", Type = FieldType.Checkbox, IsRequired = true,  Order = 5 },
                    new() { Label = "Capacidad de imitación",
                            Type = FieldType.Select,
                            Options = JsonSerializer.Serialize(new[] { "Alta", "Media", "Baja", "No evaluada" }),
                            IsRequired = true,  Order = 6 },

                    // Área comunicativa
                    new() { Label = "Nivel de comunicación verbal",
                            Type = FieldType.Select,
                            Options = JsonSerializer.Serialize(new[] { "No verbal", "Vocalizaciones", "Palabras aisladas", "Frases de 2-3 palabras", "Oraciones completas" }),
                            IsRequired = true,  Order = 7 },
                    new() { Label = "Usa comunicación aumentativa o alternativa", Type = FieldType.Checkbox, IsRequired = true,  Order = 8 },

                    // Área social y emocional
                    new() { Label = "Nivel de interacción social (1-10)",  Type = FieldType.Scale,    IsRequired = true,  Order = 9 },
                    new() { Label = "Conductas de autorregulación observadas", Type = FieldType.Textarea, IsRequired = false, Order = 10 },
                    new() { Label = "¿Se presentaron conductas disruptivas?", Type = FieldType.Checkbox, IsRequired = true,  Order = 11 },

                    // Área motora
                    new() { Label = "Motricidad gruesa",
                            Type = FieldType.Select,
                            Options = JsonSerializer.Serialize(new[] { "Adecuada para la edad", "Leve retraso", "Retraso moderado", "Retraso severo" }),
                            IsRequired = true,  Order = 12 },
                    new() { Label = "Motricidad fina",
                            Type = FieldType.Select,
                            Options = JsonSerializer.Serialize(new[] { "Adecuada para la edad", "Leve retraso", "Retraso moderado", "Retraso severo" }),
                            IsRequired = true,  Order = 13 },

                    // Cierre
                    new() { Label = "Recomendaciones y plan de intervención", Type = FieldType.Textarea, IsRequired = true,  Order = 14 },
                    new() { Label = "Nivel de urgencia de seguimiento (1-10)", Type = FieldType.Scale,   IsRequired = true,  Order = 15 },
                ]
            },
        };

        db.FormTemplates.AddRange(templates);
        await db.SaveChangesAsync();
    }
}