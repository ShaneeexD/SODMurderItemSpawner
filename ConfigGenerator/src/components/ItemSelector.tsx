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
  itemsToSpawn: string[];
  setItemsToSpawn: (items: string[]) => void;
  spawnChance: number;
  setSpawnChance: (chance: number) => void;
}

const ItemSelector: React.FC<ItemSelectorProps> = ({
  itemsToSpawn,
  setItemsToSpawn,
  spawnChance,
  setSpawnChance
}) => {
  // Handle adding a new item
  const handleAddItem = (item: string) => {
    if (!itemsToSpawn.includes(item)) {
      setItemsToSpawn([...itemsToSpawn, item]);
    }
  };

  // Handle removing an item
  const handleRemoveItem = (item: string) => {
    setItemsToSpawn(itemsToSpawn.filter(i => i !== item));
  };

  // Handle custom item input
  const [customItem, setCustomItem] = React.useState('');
  const handleAddCustomItem = () => {
    if (customItem && !itemsToSpawn.includes(customItem)) {
      setItemsToSpawn([...itemsToSpawn, customItem]);
      setCustomItem('');
    }
  };

  return (
    <Stack spacing={2}>
      <FormControl fullWidth>
        <InputLabel id="item-to-spawn-label">Items to Spawn</InputLabel>
        <Select
          labelId="item-to-spawn-label"
          id="item-to-spawn"
          value=""
          label="Items to Spawn"
          onChange={(e) => handleAddItem(e.target.value as string)}
        >
          {presetsData.InteractablePreset.map((item) => (
            <MenuItem key={item} value={item}>
              {item}
            </MenuItem>
          ))}
        </Select>
        <FormHelperText>
          Select items that will be spawned
        </FormHelperText>
      </FormControl>
      
      {/* Custom item input */}
      <Box sx={{ display: 'flex', gap: 1 }}>
        <TextField
          fullWidth
          label="Custom Item"
          value={customItem}
          onChange={(e) => setCustomItem(e.target.value)}
          onKeyPress={(e) => {
            if (e.key === 'Enter') {
              handleAddCustomItem();
            }
          }}
          size="small"
        />
      </Box>
      
      {/* Display selected items */}
      <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
        {itemsToSpawn.map((item) => (
          <Box
            key={item}
            sx={{
              display: 'flex',
              alignItems: 'center',
              bgcolor: 'primary.light',
              color: 'white',
              borderRadius: 1,
              px: 1,
              py: 0.5,
              fontSize: '0.875rem',
            }}
          >
            {item}
            <Box
              sx={{
                ml: 1,
                cursor: 'pointer',
                '&:hover': { opacity: 0.8 },
              }}
              onClick={() => handleRemoveItem(item)}
            >
              ✕
            </Box>
          </Box>
        ))}
      </Box>

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
