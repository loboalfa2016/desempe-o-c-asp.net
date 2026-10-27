INSERT INTO "Instructors" ("FullName", "Email", "Specialty")
SELECT data."FullName", data."Email", data."Specialty"
FROM (VALUES
    ('Andrés Felipe Mejía Arango', 'andres.mejia@correo.com', 'Fotografía'),
    ('Rosa Elena Barrios Polo', 'rosa.barrios@correo.com', 'Cocina'),
    ('Mauricio Jiménez Cárdenas', 'mauricio.jimenez@correo.com', 'Oratoria'),
    ('Natalia Herrera Castaño', 'natalia.herrera@correo.com', 'Diseño gráfico'),
    ('Hernán Darío Quintero Gil', 'hernan.quintero@correo.com', 'Música'),
    ('Paola Andrea Vargas Mora', 'paola.vargas@correo.com', 'Marketing digital')
) AS data("FullName", "Email", "Specialty")
WHERE NOT EXISTS (
    SELECT 1
    FROM "Instructors" existing
    WHERE existing."Email" = data."Email"
);
