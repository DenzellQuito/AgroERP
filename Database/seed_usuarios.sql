-- =====================================================================
-- seed_usuarios.sql (AgroERP) - Usuarios de prueba. Contrasena de todos: 12345
-- Ejecutar en SSMS sobre LA BASE DEL PROYECTO, DESPUES de Update-Database.
-- Solo inserta los Id que no existan (no pisa nada).
-- =====================================================================
INSERT INTO dbo.AspNetUsers
 (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed,
  PasswordHash, SecurityStamp, ConcurrencyStamp,
  PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount)
SELECT v.Id, v.U, UPPER(v.U), v.E, UPPER(v.E), 1,
       '12345', CONVERT(varchar(36), NEWID()), CONVERT(varchar(36), NEWID()),
       0, 0, 0, 0
FROM (VALUES
  ('USR-001', 'denzell.admin', 'gerencia@gramadal.com'),
  ('USR-002', 'changa',        'changa@gramadal.com'),
  ('USR-003', 'lavado',        'lavado@gramadal.com'),
  ('USR-004', 'mendez',        'mendez@gramadal.com'),
  ('USR-005', 'diaz',          'diaz@gramadal.com')
) AS v(Id, U, E)
WHERE NOT EXISTS (SELECT 1 FROM dbo.AspNetUsers a WHERE a.Id = v.Id);

SELECT Id, UserName, Email FROM dbo.AspNetUsers;
