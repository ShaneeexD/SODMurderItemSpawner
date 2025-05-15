import React from 'react';
import {
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  FormHelperText,
  Box,
  Chip,
  Typography,
  Paper
} from '@mui/material';
import type { SelectChangeEvent } from '@mui/material';

interface RandomLocationSelectorProps {
  randomLocations: string[];
  onChange: (locations: string[]) => void;
}

const RandomLocationSelector: React.FC<RandomLocationSelectorProps> = ({ randomLocations, onChange }) => {
  // Handle array field changes (for multi-select)
  const handleArrayChange = (event: SelectChangeEvent<string[]>) => {
    const value = event.target.value;
    onChange(typeof value === 'string' ? value.split(',') : value);
  };

  // Handle removing an item from the array
  const handleRemoveLocation = (locationToRemove: string) => {
    const newLocations = randomLocations.filter(location => location !== locationToRemove);
    onChange(newLocations);
  };

  return (
    <Box>
      <Paper elevation={2} sx={{ p: 2, mb: 2 }}>
        <Typography variant="subtitle1" gutterBottom>Random Location Options</Typography>
        <FormControl fullWidth sx={{ mt: 1 }}>
          <InputLabel id="random-locations-label">Possible Locations</InputLabel>
          <Select
            labelId="random-locations-label"
            id="random-locations"
            multiple
            value={randomLocations || []}
            onChange={handleArrayChange}
            renderValue={(selected) => (
              <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                {(selected as string[]).map((value) => (
                  <Chip 
                    key={value} 
                    label={value} 
                    onDelete={() => handleRemoveLocation(value)}
                    onMouseDown={(event) => event.stopPropagation()}
                  />
                ))}
              </Box>
            )}
          >
            <MenuItem value="Mailbox">Mailbox</MenuItem>
            <MenuItem value="Doormat">Doormat</MenuItem>
            <MenuItem value="HomeLobby">Home Lobby</MenuItem>
            <MenuItem value="WorkplaceLobby">Workplace Lobby</MenuItem>
            <MenuItem value="HomeBuildingEntrance">Home Building Entrance</MenuItem>
            <MenuItem value="WorkplaceBuildingEntrance">Workplace Building Entrance</MenuItem>
          </Select>
          <FormHelperText>Select possible random locations</FormHelperText>
        </FormControl>
      </Paper>
    </Box>
  );
};

export default RandomLocationSelector;
