import json

with open('monetarias.json', 'r', encoding='utf-16') as f:
    data = json.load(f)

for item in data.get('results', []):
    desc = item.get('descripcion', '').upper()
    if 'CER' in desc or 'COEFICIENTE' in desc or 'ESTABILIZA' in desc:
        print(f"ID: {item['idVariable']} - {item['descripcion']} (Desde: {item['primerFechaInformada']}, Hasta: {item['ultFechaInformada']})")
