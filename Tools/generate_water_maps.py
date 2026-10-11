"""Generate seamless linear-data shader inputs, not screenshots. numpy, scipy, Pillow."""
from pathlib import Path
import numpy as np
from scipy.spatial import cKDTree
from PIL import Image

OUT = Path(__file__).resolve().parents[1] / 'Assets/GameWork/ArchipelagoKit/Runtime/Water/Textures'
OUT.mkdir(parents=True, exist_ok=True)
N = 512
y, x = np.mgrid[0:N, 0:N].astype(float) / N
rng = np.random.default_rng(1741)

def periodic_field(low, high, modes, directional=False):
    value = np.zeros((N,N))
    for _ in range(modes):
        kx = int(rng.integers(low, high+1))
        ky = int(rng.integers(low, high+1))
        if directional:
            ky = int(kx * rng.uniform(-.35,1.25)) or 1
        elif rng.random() < .5:
            ky *= -1
        value += np.sin(2*np.pi*(kx*x + ky*y) + rng.uniform(0,2*np.pi)) / np.hypot(kx,ky)**1.15
    return value

def norm(v):
    return (v-v.min()) / max(1e-9, v.max()-v.min())

def save(name, channels):
    image = np.stack(channels, axis=-1)
    assert np.isfinite(image).all()
    Image.fromarray(np.uint8(np.clip(image,0,1)*255+.5)).save(OUT/name)

h = periodic_field(3,18,60,True) + periodic_field(12,44,55)*.7 + periodic_field(30,70,40)*.15
dx = (np.roll(h,-1,axis=1)-np.roll(h,1,axis=1))*.5
dy = (np.roll(h,-1,axis=0)-np.roll(h,1,axis=0))*.5
scale = max(np.max(np.abs(dx)),np.max(np.abs(dy)))
save('WaveData.png', [.5-dx/scale*.48,.5-dy/scale*.48,norm(h)])
macro = [norm(periodic_field(1,5,34)), norm(periodic_field(1,7,40)), norm(periodic_field(2,9,36))]
save('MacroVariation.png', macro)
warp_x = periodic_field(1,4,24)*.021
warp_y = periodic_field(1,4,24)*.021
coords = np.stack(((x+warp_x)%1,(y+warp_y)%1),axis=-1).reshape(-1,2)
dist,_ = cKDTree(rng.random((310,2)), boxsize=1).query(coords,k=2)
gap = (dist[:,1]-dist[:,0]).reshape(N,N)
edges = np.exp(-(gap/.0038)**2)
foamNoise = norm(periodic_field(3,14,50))
lace = edges*np.clip((foamNoise-.2)/.38,0,1)
caustic = np.exp(-(gap/.0019)**2)
save('FoamPattern.png',[lace,foamNoise,caustic])
print('Created three 512 x 512 seamless data textures:',OUT)
