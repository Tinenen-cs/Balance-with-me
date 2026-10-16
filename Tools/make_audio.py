"""Generate original loop music and game cues using Python's standard library."""
from pathlib import Path
import array
import math
import random
import sys
import wave

ROOT = Path(__file__).resolve().parents[1] / "Assets" / "Audio"
RATE = 24000


def frequency(note):
    return 440 * 2 ** ((note - 69) / 12)


def tone(samples, start, duration, note, volume=.18):
    first = int(start * RATE)
    for i in range(int(duration * RATE)):
        if first + i >= len(samples):
            break
        t = i / RATE
        envelope = min(1, t / .008) * math.exp(-t * 5 / duration) * min(1, (duration - t) / .03)
        phase = 2 * math.pi * frequency(note) * t
        samples[first+i] += volume * envelope * (math.sin(phase) + .2 * math.sin(phase*2))


def save(folder, name, samples):
    path = ROOT / folder / (name + ".wav")
    path.parent.mkdir(parents=True, exist_ok=True)
    pcm = array.array("h", (int(max(-.95, min(.95, x)) * 32767) for x in samples))
    if sys.byteorder != "little":
        pcm.byteswap()
    with wave.open(str(path), "wb") as output:
        output.setparams((1, 2, RATE, len(pcm), "NONE", "not compressed"))
        output.writeframes(pcm.tobytes())
    print(path.relative_to(ROOT.parent.parent))


def music(name, roots, melody, bpm):
    beat = 60 / bpm
    samples = [0.] * int(beat * 16 * RATE)
    for bar, root in enumerate(roots):
        for chord_note in (root, root+4, root+7):
            tone(samples, bar*4*beat, 3.8*beat, chord_note, .07)
        for step in range(8):
            note = melody[(bar*8+step) % len(melody)]
            tone(samples, (bar*4+step*.5)*beat, .45*beat, note, .14)
        for step in range(4):
            tone(samples, (bar*4+step)*beat, .6*beat, root-12, .10)
    # All envelopes end before the loop boundary, avoiding discontinuities.
    save("Music", name, samples)


def jingle(name, notes, step=.12):
    samples = [0.] * int((len(notes)*step+.25)*RATE)
    for i, note in enumerate(notes):
        tone(samples, i*step, step*1.8, note, .3)
    save("SFX", name, samples)


def sweep(name, start, end, duration, noise=.1):
    rng = random.Random(name)
    samples = []
    phase = 0
    for i in range(int(duration*RATE)):
        t = i / RATE
        progress = t / duration
        phase += 2 * math.pi * (start+(end-start)*progress) / RATE
        envelope = math.sin(math.pi*progress) * math.exp(-progress*2)
        samples.append(.5*envelope*(math.sin(phase)+noise*rng.uniform(-1,1)))
    save("SFX", name, samples)


if __name__ == "__main__":
    music("sky_breeze", [48,53,55,48], [72,76,79,76,74,77,79,77], 100)
    music("playful_balance", [55,60,57,55], [79,83,86,83,81,84,88,84], 120)
    sweep("jump", 220, 650, .2)
    sweep("punch", 260, 80, .13, .55)
    sweep("push", 180, 95, .32, .35)
    sweep("hit", 150, 45, .18, .7)
    sweep("shove", 300, 130, .16, .35)
    jingle("start", [60,64,67], .1)
    jingle("win", [72,76,79,84], .17)
    jingle("draw", [67,64,60], .2)
    jingle("emote_happy", [76,79,84], .11)
    jingle("emote_sad", [67,63,60], .14)
