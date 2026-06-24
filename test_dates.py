import urllib.request
import urllib.error
import datetime

for i in range(30):
    dt = datetime.date(2026, 6, 24) - datetime.timedelta(days=i)
    url = f"https://api.bcra.gob.ar/estadisticas/v4.0/datosvariable/30/2026-05-01/{dt.strftime('%Y-%m-%d')}"
    req = urllib.request.Request(url, headers={'User-Agent': 'Mozilla/5.0'})
    try:
        urllib.request.urlopen(req)
        print(f"Success with {dt.strftime('%Y-%m-%d')}!")
        break
    except urllib.error.HTTPError as e:
        print(f"{dt.strftime('%Y-%m-%d')} -> {e.code}")
    except Exception as e:
        print(f"{dt.strftime('%Y-%m-%d')} -> {e}")
