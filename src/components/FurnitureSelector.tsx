import React from 'react';
import { 
  FormControl, 
  InputLabel, 
  Select, 
  MenuItem, 
  FormHelperText,
  Stack,
  Chip,
  OutlinedInput,
  Box,
  TextField,
  Button,
  ListItemText,
  Checkbox
} from '@mui/material';
import furniturePresetsData from '../presets.json';

interface FurnitureSelectorProps {
  furniturePresets: string[];
  setFurniturePresets: (presets: string[]) => void;
  customFurniture: string;
  setCustomFurniture: (furniture: string) => void;
}

const FurnitureSelector: React.FC<FurnitureSelectorProps> = ({
  furniturePresets,
  setFurniturePresets,
  customFurniture,
  setCustomFurniture
}) => {
  // Ensure furniturePresets is always an array
  const safePresets = furniturePresets || [];
  
  // Handle selection change
  const handleSelectionChange = (event: any) => {
    const {
      target: { value },
    } = event;
    setFurniturePresets(typeof value === 'string' ? value.split(',') : value);
  };

  // Handle removing a selected item
  const handleDelete = (valueToDelete: string) => (event: React.MouseEvent) => {
    event.stopPropagation();
    const newPresets = safePresets.filter(item => item !== valueToDelete);
    setFurniturePresets(newPresets);
  };

  // Handle adding a custom furniture item
  const handleAddCustomFurniture = () => {
    if (customFurniture && !safePresets.includes(customFurniture)) {
      setFurniturePresets([...safePresets, customFurniture]);
      setCustomFurniture('');
    }
  };

  return (
    <Stack spacing={2}>
      <FormControl fullWidth>
        <InputLabel id="furniture-presets-label">Furniture Presets</InputLabel>
        <Select
          labelId="furniture-presets-label"
          id="furniture-presets"
          multiple
          value={safePresets}
          onChange={handleSelectionChange}
          input={<OutlinedInput label="Furniture Presets" />}
          renderValue={(selected) => {
            if ((selected as string[]).length === 0) {
              return <em>No furniture selected</em>;
            }
            return (
              <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                {(selected as string[]).map((value) => (
                  <Chip 
                    key={value} 
                    label={value} 
                    onDelete={handleDelete(value)}
                    onClick={(e) => e.stopPropagation()}
                  />
                ))}
              </Box>
            );
          }}
          MenuProps={{
            PaperProps: {
              style: {
                maxHeight: 300
              },
            },
          }}
        >
          {furniturePresetsData.FurniturePreset.map((furniture) => (
            <MenuItem key={furniture} value={furniture}>
              <Checkbox checked={safePresets.indexOf(furniture) > -1} />
              <ListItemText primary={furniture} />
            </MenuItem>
          ))}
        </Select>
        <FormHelperText>
          Select the furniture types where the item can be placed
        </FormHelperText>
      </FormControl>

      <Box sx={{ display: 'flex', gap: 2 }}>
        <Box sx={{ flexGrow: 1 }}>
          <TextField
            fullWidth
            label="Custom Furniture Type"
            value={customFurniture}
            onChange={(e) => setCustomFurniture(e.target.value)}
            helperText="Add a custom furniture type if not in the list above"
            variant="outlined"
          />
        </Box>
        <Box>
          <Button 
            variant="contained" 
            onClick={handleAddCustomFurniture}
            sx={{ height: '56px' }}
            fullWidth
          >
            Add
          </Button>
        </Box>
      </Box>
    </Stack>
  );
};

export default FurnitureSelector;
