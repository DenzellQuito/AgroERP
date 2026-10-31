"""Microservicio de pronóstico de cosecha (XGBoost) - Mi Paraíso Gramadal.
Ejecutar:  uvicorn app:app --port 8000
Si existe 'historial.csv' (columnas: mes,hectareas,jabas) se entrena con él;
si no, usa datos sintéticos de demostración."""
import os
import numpy as np
import pandas as pd
from fastapi import FastAPI
from pydantic import BaseModel, Field
from xgboost import XGBRegressor

app = FastAPI(title="Forecast Gramadal")
CSV = os.path.join(os.path.dirname(__file__), "historial.csv")


def historial_sintetico(n=600, seed=42) -> pd.DataFrame:
    rng = np.random.default_rng(seed)
    estacion = {1: 1.15, 2: 1.2, 3: 1.1, 4: 0.9, 5: 0.75, 6: 0.6,
                7: 0.55, 8: 0.65, 9: 0.85, 10: 1.0, 11: 1.1, 12: 1.2}
    filas = []
    for _ in range(n):
        mes = int(rng.integers(1, 13))
        ha = float(rng.uniform(1, 30))
        jabas = ha * 800 * estacion[mes] * rng.normal(1, 0.07)
        filas.append((mes, ha, max(jabas, 0)))
    return pd.DataFrame(filas, columns=["mes", "hectareas", "jabas"])


def entrenar():
    if os.path.exists(CSV):
        df, origen = pd.read_csv(CSV), "csv"
    else:
        df, origen = historial_sintetico(), "sintetico"
    modelo = XGBRegressor(n_estimators=200, max_depth=4, learning_rate=0.08)
    modelo.fit(df[["mes", "hectareas"]], df["jabas"])
    return modelo, origen, len(df)


MODELO, ORIGEN, FILAS = entrenar()


class ForecastIn(BaseModel):
    hectareas: float = Field(gt=0)
    mes: int = Field(ge=1, le=12)


@app.get("/health")
def health():
    return {"ok": True, "datos": ORIGEN, "filas": FILAS}


@app.post("/forecast")
def forecast(req: ForecastIn):
    x = pd.DataFrame([[req.mes, req.hectareas]], columns=["mes", "hectareas"])
    jabas = float(MODELO.predict(x)[0])
    return {"volumen_estimado": max(int(round(jabas)), 0), "modelo": f"xgboost-{ORIGEN}"}
