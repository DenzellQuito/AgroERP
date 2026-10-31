-- seed_kpis.sql (AgroERP) - Los 6 indicadores del dashboard. Sobre LA BASE DEL PROYECTO. Se puede repetir.
INSERT INTO dbo.IndicadoresKPI (NombreMetrica, MetaObjetivo, ToleranciaAlarma)
SELECT v.N, v.M, v.T FROM (VALUES
  (N'OTIF',                        95, 5),
  (N'Cumplimiento de Proveedores', 90, 10),
  (N'Rotación de Inventario',       3, 1),
  (N'Exactitud de Inventario',     98, 3),
  (N'Costo Logístico',              5, 2),
  (N'Merma',                        3, 2)) v(N, M, T)
WHERE NOT EXISTS (SELECT 1 FROM dbo.IndicadoresKPI k WHERE k.NombreMetrica = v.N);

SELECT * FROM dbo.IndicadoresKPI;
