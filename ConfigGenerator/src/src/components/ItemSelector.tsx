import React from 'react';
import { 
  FormControl, 
  InputLabel, 
  Select, 
  MenuItem, 
  FormHelperText,
  TextField,
  Stack,
  Slider,
  Typography,
  Box
} from '@mui/material';
import presetsData from '../presets.json';

interface ItemSelectorProps {
  itemToSpawn: string;
  setItemToSpawn: (item: string) => void;
  spawnChance: number;
  setSpawnChance: (chance: number) => void;
}

const ItemSelector: React.FC<ItemSelectorProps> = ({
  itemToSpawn,
  setItemToSpawn,
  spawnChance,
  setSpawnChance
}) => {
  return (
    <Stack spacing={2}>
      <FormControl fullWidth>
        <InputLabel id="item-to-spawn-label">Item to Spawn</InputLabel>
        <Select
          labelId="item-to-spawn-label"
          id="item-to-spawn"
          value={itemToSpawn}
          label="Item to Spawn"
          onChange={(e) => setItemToSpawn(e.target.value)}
        >
          {presetsData.InteractablePreset.map((item) => (
            <MenuItem key={item} value={item}>
              {item}
            </MenuItem>
          ))}
        </Select>
        <FormHelperText>
          Select the item that will be spawned
        </FormHelperText>
      </FormControl>

      <Box>
        <Typography id="spawn-chance-slider" gutterBottom>
          Spawn Chance: {spawnChance.toFixed(2)}
        </Typography>
        <Slider
          value={spawnChance}
          onChange={(_, newValue) => setSpawnChance(newValue as number)}
          aria-labelledby="spawn-chance-slider"
          valueLabelDisplay="auto"
          step={0.05}
          marks
          min={0}
          max={1}
        />
        <FormHelperText>
          Set the probability of the item spawning (0 = never, 1 = always)
        </FormHelperText>
      </Box>

      <TextField
        fullWidth
        label="Custom Item Name"
        helperText="Optional: Enter a custom item name if not in the list above"
        variant="outlined"
        onChange={(e) => {
          if (e.target.value) {
            setItemToSpawn(e.target.value);
          }
        }}
      />
    </Stack>
  );
};

export default ItemSelector;
