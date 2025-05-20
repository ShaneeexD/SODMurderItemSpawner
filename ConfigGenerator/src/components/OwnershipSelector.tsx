import React from 'react';
import { 
  FormControl, 
  InputLabel, 
  Select, 
  MenuItem, 
  FormHelperText,
  Stack,
  Typography,
  Paper,
  Box
} from '@mui/material';
import { BelongsTo } from '../models/configTypes';

interface OwnershipSelectorProps {
  belongsTo: BelongsTo;
  onBelongsToChange: (owner: BelongsTo) => void;
  spawnLocationRecipient: BelongsTo;
  onSpawnLocationRecipientChange: (recipient: BelongsTo) => void;
}

const OwnershipSelector: React.FC<OwnershipSelectorProps> = ({
  belongsTo,
  onBelongsToChange,
  spawnLocationRecipient,
  onSpawnLocationRecipientChange
}) => {
  // Convert enum to array for rendering
  const ownershipTypes = Object.keys(BelongsTo)
    .filter(key => isNaN(Number(key)))
    .map(key => ({
      value: BelongsTo[key as keyof typeof BelongsTo],
      label: key
    }));

  return (
    <Paper elevation={1} sx={{ p: 2 }}>
      <Typography variant="subtitle1" gutterBottom>
        Item Ownership Settings
      </Typography>
      
      <Stack spacing={2}>
        <Box sx={{ display: 'flex', flexDirection: { xs: 'column', md: 'row' }, gap: 2 }}>
          <Box sx={{ flex: 1 }}>
            <FormControl fullWidth>
              <InputLabel id="belongs-to-label">Item Owner</InputLabel>
              <Select
                labelId="belongs-to-label"
                id="belongs-to"
                value={belongsTo}
                label="Item Owner"
                onChange={(e) => onBelongsToChange(Number(e.target.value) as BelongsTo)}
              >
                {ownershipTypes.map((type) => (
                  <MenuItem key={type.value} value={type.value}>
                    {type.label}
                  </MenuItem>
                ))}
              </Select>
              <FormHelperText>
                Who owns the item (whose prints will appear on it)
              </FormHelperText>
            </FormControl>
          </Box>
          
          <Box sx={{ flex: 1 }}>
            <FormControl fullWidth>
              <InputLabel id="spawn-location-recipient-label">Location Recipient</InputLabel>
              <Select
                labelId="spawn-location-recipient-label"
                id="spawn-location-recipient"
                value={spawnLocationRecipient}
                label="Location Recipient"
                onChange={(e) => onSpawnLocationRecipientChange(Number(e.target.value) as BelongsTo)}
              >
                {ownershipTypes.map((type) => (
                  <MenuItem key={type.value} value={type.value}>
                    {type.label}
                  </MenuItem>
                ))}
              </Select>
              <FormHelperText>
                Whose location to use for spawning (e.g., whose home/workplace)
              </FormHelperText>
            </FormControl>
          </Box>
        </Box>
      </Stack>
    </Paper>
  );
};

export default OwnershipSelector;
