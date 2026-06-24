import urllib.request
import urllib.error

url = "https://apis.datos.gob.ar/series/api/series/?ids=148.3_INIVELGENERAL_DICI_M_26&start_date=2026-02-01&end_date=2026-05-31&collapse=month&format=json"
req = urllib.request.Request(url, headers={'User-Agent': 'Mozilla/5.0'})
try:
    print(urllib.request.urlopen(req).read().decode('utf-8'))
except urllib.error.HTTPError as e:
    print(e.read().decode('utf-8'))
