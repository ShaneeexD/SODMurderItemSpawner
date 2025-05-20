import React from 'react';
import { 
  FormControlLabel, 
  Switch, 
  Typography, 
  Box, 
  Paper,
  TextField,
  Tooltip,
  Collapse,
  Stack
} from '@mui/material';
import InfoIcon from '@mui/icons-material/Info';

interface SpawnRequirementsSelectorProps {
  requiresPriorItem: boolean;
  requiredPriorItem: string;
  requiresSeparateTrigger: boolean;
  requiresMultipleTriggers: boolean;
  requiredTriggerCount: number;
  onChange: (values: {
    requiresPriorItem: boolean;
    requiredPriorItem: string;
    requiresSeparateTrigger: boolean;
    requiresMultipleTriggers: boolean;
    requiredTriggerCount: number;
  }) => void;
}

const SpawnRequirementsSelector: React.FC<SpawnRequirementsSelectorProps> = ({ 
  requiresPriorItem,
  requiredPriorItem,
  requiresSeparateTrigger,
  requiresMultipleTriggers,
  requiredTriggerCount,
  onChange 
}) => {
  const handleChange = (field: string, value: any) => {
    onChange({
      requiresPriorItem,
      requiredPriorItem,
      requiresSeparateTrigger,
      requiresMultipleTriggers,
      requiredTriggerCount,
      [field]: value
    });
  };

  return (
    <Paper elevation={2} sx={{ p: 2, mb: 2 }}>
      <Typography variant="h6" gutterBottom>
        Spawn Requirements
      </Typography>
      
      <Stack spacing={2}>
        {/* Prior Item Requirements */}
        <Box>
          <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
            <FormControlLabel
              control={
                <Switch
                  checked={requiresPriorItem}
                  onChange={(e) => handleChange('requiresPriorItem', e.target.checked)}
                  color="primary"
                />
              }
              label="Requires a prior item to have spawned"
            />
            <Tooltip title="When enabled, this item will only spawn if another item has already been spawned.">
              <InfoIcon color="action" fontSize="small" sx={{ ml: 1 }} />
            </Tooltip>
          </Box>
          
          <Collapse in={requiresPriorItem}>
            <Box sx={{ pl: 4, pr: 2, mb: 2, display: 'flex', flexDirection: 'column', gap: 2 }}>
              <Box>
                <TextField
                  fullWidth
                  label="Required Prior Item Rule Name"
                  value={requiredPriorItem}
                  onChange={(e) => handleChange('requiredPriorItem', e.target.value)}
                  helperText="Enter the rule name (not the interactable name) of the item that must spawn first"
                  variant="outlined"
                  size="small"
                />
              </Box>
              
              <Box>
                <FormControlLabel
                  control={
                    <Switch
                      checked={requiresSeparateTrigger}
                      onChange={(e) => handleChange('requiresSeparateTrigger', e.target.checked)}
                      color="primary"
                    />
                  }
                  label="Requires a separate trigger event"
                />
                <Tooltip title="If enabled, the system will not check within the same event as the required prior item spawning.">
                  <InfoIcon color="action" fontSize="small" sx={{ ml: 1 }} />
                </Tooltip>
              </Box>
            </Box>
          </Collapse>
        </Box>
        
        {/* Multiple Triggers Requirements */}
        <Box>
          <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
            <FormControlLabel
              control={
                <Switch
                  checked={requiresMultipleTriggers}
                  onChange={(e) => handleChange('requiresMultipleTriggers', e.target.checked)}
                  color="primary"
                />
              }
              label="Requires multiple trigger events"
            />
            <Tooltip title="When enabled, this item will only spawn after the event has been triggered multiple times.">
              <InfoIcon color="action" fontSize="small" sx={{ ml: 1 }} />
            </Tooltip>
          </Box>
          
          <Collapse in={requiresMultipleTriggers}>
            <Box sx={{ pl: 4, pr: 2 }}>
              <TextField
                type="number"
                label="Required Trigger Count"
                value={requiredTriggerCount}
                onChange={(e) => handleChange('requiredTriggerCount', parseInt(e.target.value) || 2)}
                helperText="How many times the event needs to be fired to spawn the item"
                variant="outlined"
                size="small"
                inputProps={{ min: 2 }}
              />
            </Box>
          </Collapse>
        </Box>
      </Stack>
    </Paper>
  );
};

export default SpawnRequirementsSelector;
