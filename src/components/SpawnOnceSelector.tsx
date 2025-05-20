import React from 'react';
import { 
  FormControlLabel, 
  Switch, 
  Typography, 
  Box, 
  Paper,
  Tooltip
} from '@mui/material';
import InfoIcon from '@mui/icons-material/Info';

interface SpawnOnceSelectorProps {
  onlySpawnOnce: boolean;
  onChange: (onlySpawnOnce: boolean) => void;
}

const SpawnOnceSelector: React.FC<SpawnOnceSelectorProps> = ({ 
  onlySpawnOnce, 
  onChange 
}) => {
  return (
    <Paper elevation={2} sx={{ p: 2, mb: 2 }}>
      <Typography variant="h6" gutterBottom>
        Spawn Frequency
      </Typography>
      
      <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
        <FormControlLabel
          control={
            <Switch
              checked={onlySpawnOnce}
              onChange={(e) => onChange(e.target.checked)}
              color="primary"
            />
          }
          label="Only spawn once per murder case"
        />
        <Tooltip title="When enabled, this item will only spawn once during a murder case, even if the trigger events occur multiple times.">
          <InfoIcon color="action" fontSize="small" sx={{ ml: 1 }} />
        </Tooltip>
      </Box>
    </Paper>
  );
};

export default SpawnOnceSelector;
