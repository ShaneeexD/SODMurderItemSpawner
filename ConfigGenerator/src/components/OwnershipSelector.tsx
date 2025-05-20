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
  Box,
  FormControlLabel,
  Switch
} from '@mui/material';
import { BelongsTo } from '../models/configTypes';

interface OwnershipSelectorProps {
  belongsTo: BelongsTo;
  onBelongsToChange: (owner: BelongsTo) => void;
  spawnLocationRecipient: BelongsTo;
  onSpawnLocationRecipientChange: (recipient: BelongsTo) => void;
  // Multiple owners
  useMultipleOwners: boolean;
  onUseMultipleOwnersChange: (useMultiple: boolean) => void;
  owners: BelongsTo[];
  onOwnersChange: (owners: BelongsTo[]) => void;
}

const OwnershipSelector: React.FC<OwnershipSelectorProps> = ({
  belongsTo,
  onBelongsToChange,
  spawnLocationRecipient,
  onSpawnLocationRecipientChange,
  useMultipleOwners,
  onUseMultipleOwnersChange,
  owners,
  onOwnersChange
}) => {
  // Convert enum to array for rendering
  const ownershipTypes = Object.keys(BelongsTo)
    .filter(key => isNaN(Number(key)))
    .map(key => ({
      value: BelongsTo[key as keyof typeof BelongsTo],
      label: key
    }));

  // Handle adding and removing owners
  const handleToggleOwner = (ownerValue: BelongsTo) => {
    if (owners.includes(ownerValue)) {
      // Remove owner if already in the list
      onOwnersChange(owners.filter(o => o !== ownerValue));
    } else {
      // Add owner if not in the list
      onOwnersChange([...owners, ownerValue]);
    }
  };

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
                Whose location to use for spawning
              </FormHelperText>
            </FormControl>
          </Box>
        </Box>
        
        {/* Multiple Owners Section */}
        <Box sx={{ mt: 2 }}>
          <FormControlLabel
            control={
              <Switch
                checked={useMultipleOwners}
                onChange={(e) => onUseMultipleOwnersChange(e.target.checked)}
                color="primary"
              />
            }
            label="Use Multiple Owners"
          />
          
          {useMultipleOwners && (
            <Box sx={{ mt: 1, border: '1px solid #e0e0e0', borderRadius: 1, p: 2 }}>
              <Typography variant="subtitle2" gutterBottom>
                Select Multiple Owners
              </Typography>
              <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
                {ownershipTypes.map((type) => (
                  <FormControlLabel
                    key={type.value}
                    control={
                      <Switch
                        size="small"
                        checked={owners.includes(type.value)}
                        onChange={() => handleToggleOwner(type.value)}
                        color="primary"
                      />
                    }
                    label={type.label}
                  />
                ))}
              </Box>
              <FormHelperText>
                Select all owners that apply to this item
              </FormHelperText>
            </Box>
          )}
        </Box>
      </Stack>
    </Paper>
  );
};

export default OwnershipSelector;
