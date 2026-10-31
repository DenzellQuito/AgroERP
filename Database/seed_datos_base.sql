-- =====================================================================
-- seed_datos_base.sql  (AgroERP) - Datos base de prueba
-- Ejecutar en SSMS sobre LA BASE DEL PROYECTO (no en master), DESPUES de Update-Database.
-- Se puede ejecutar varias veces: no duplica filas.
-- =====================================================================

-- 1) Articulos: detecta si Tipo es texto o numero (enum sin conversion)
DECLARE @tipoNum BIT = CASE WHEN EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME='Articulos' AND COLUMN_NAME='Tipo'
      AND DATA_TYPE IN ('int','tinyint','smallint')) THEN 1 ELSE 0 END;

IF @tipoNum = 1
  EXEC(N'INSERT INTO dbo.Articulos (Codigo,Nombre,Tipo,UnidadMedida)
    SELECT v.C,v.N,v.T,v.U FROM (VALUES
     (''FRE-001'',''Fresa fresca'',0,''Jaba''),
     (''AGR-001'',''Fungicida'',1,''Litro''),
     (''AGR-002'',''Urea'',1,''Kg''),
     (''EMP-001'',''Jaba plastica vacia'',2,''Unidad'')) v(C,N,T,U)
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Articulos a WHERE a.Codigo=v.C)');
ELSE
  EXEC(N'INSERT INTO dbo.Articulos (Codigo,Nombre,Tipo,UnidadMedida)
    SELECT v.C,v.N,v.T,v.U FROM (VALUES
     (''FRE-001'',''Fresa fresca'',''Fresa'',''Jaba''),
     (''AGR-001'',''Fungicida'',''Insumo'',''Litro''),
     (''AGR-002'',''Urea'',''Insumo'',''Kg''),
     (''EMP-001'',''Jaba plastica vacia'',''Empaque'',''Unidad'')) v(C,N,T,U)
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Articulos a WHERE a.Codigo=v.C)');

DECLARE @fre INT = (SELECT Id FROM dbo.Articulos WHERE Codigo='FRE-001');
DECLARE @fun INT = (SELECT Id FROM dbo.Articulos WHERE Codigo='AGR-001');
DECLARE @ure INT = (SELECT Id FROM dbo.Articulos WHERE Codigo='AGR-002');
DECLARE @jab INT = (SELECT Id FROM dbo.Articulos WHERE Codigo='EMP-001');

-- 2) Parametros de inventario
INSERT INTO dbo.ParametrosInventario (ArticuloId, StockSeguridad, LoteEconomico)
SELECT v.A, v.S, v.L FROM (VALUES (@fun,20,50),(@ure,50,100),(@jab,500,1000)) v(A,S,L)
WHERE NOT EXISTS (SELECT 1 FROM dbo.ParametrosInventario p WHERE p.ArticuloId = v.A);

-- 3) Receta (BOM) de marzo: el nombre de la tabla se detecta solo
DECLARE @rec sysname = (SELECT TOP 1 name FROM sys.tables WHERE name LIKE 'Receta%');
DECLARE @sql NVARCHAR(MAX) =
 N'INSERT INTO dbo.' + QUOTENAME(@rec) + N' (ArticuloFinalId,ArticuloInsumoId,MesAplicacion,CantidadPorJaba)
   SELECT ' + CAST(@fre AS NVARCHAR(10)) + N', v.I, 3, v.Q FROM (VALUES ('
   + CAST(@fun AS NVARCHAR(10)) + N',0.02),(' + CAST(@ure AS NVARCHAR(10)) + N',0.05),('
   + CAST(@jab AS NVARCHAR(10)) + N',1.0)) v(I,Q)
   WHERE NOT EXISTS (SELECT 1 FROM dbo.' + QUOTENAME(@rec) + N' r
                     WHERE r.ArticuloInsumoId = v.I AND r.MesAplicacion = 3)';
EXEC(@sql);

-- 4) Campania
INSERT INTO dbo.CampaniasAgricolas (Nombre, Estado, HectareasActivas, MesProyectado)
SELECT N'Campaña Verano 2026', 'Activa', 10, 3
WHERE NOT EXISTS (SELECT 1 FROM dbo.CampaniasAgricolas WHERE Nombre = N'Campaña Verano 2026');

-- Verificacion
SELECT * FROM dbo.Articulos;
SELECT * FROM dbo.ParametrosInventario;
SELECT * FROM dbo.CampaniasAgricolas;
