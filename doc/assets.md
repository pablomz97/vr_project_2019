# Asset Creation Guidelines

## Meshes
- All Meshes should have at least one non-overlapping UV channel, this is helpful for lightmapping
- All Meshes should be built with corectly scaled units, level designers shouldnt have to constantly change the scale of objects when building levels

## Textures
- Texture resolution on each axis should always be a power of 2, for example 128x512, 1024x1024, 2048x1024, 1024x2048, 1x512, etc.
- resolutions above 4096x4096 are not recommended
- Texture resolution should be chosen such that texel density is roughly similar for all assets (eg. if an asset is 2x bigger then another one, it needs 2x the texture resolution etc.)

## Audio
- Audio files should be encoded as .wav, .ogg or .mp3, .wav is only recommended for very short audio files
- The compression format in Unity should be set to ADPCM or Vorbis for larger files, for very small files PCM can be used

