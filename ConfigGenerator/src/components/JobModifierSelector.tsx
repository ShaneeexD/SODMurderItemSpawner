import React, { useState } from 'react';
import {
  Box,
  FormControl,
  FormControlLabel,
  Switch,
  Typography,
  Button,
  Select,
  MenuItem,
  Chip,
  Stack,
  IconButton,
  InputLabel,
  Paper
} from '@mui/material';
import DeleteIcon from '@mui/icons-material/Delete';
import AddIcon from '@mui/icons-material/Add';
import { BelongsTo, JobRule } from '../models/configTypes';
import type { JobModifier } from '../models/configTypes';
import presetsData from '../presets.json';

interface JobModifierSelectorProps {
  useJobModifiers: boolean;
  onUseJobModifiersChange: (value: boolean) => void;
  jobModifiers: JobModifier[];
  onJobModifiersChange: (modifiers: JobModifier[]) => void;
}

const JobModifierSelector: React.FC<JobModifierSelectorProps> = ({
  useJobModifiers,
  onUseJobModifiersChange,
  jobModifiers,
  onJobModifiersChange
}) => {
  const [selectedJob, setSelectedJob] = useState<string>('');

  // Get job list from presets
  const jobList = presetsData.OccupationPreset || [];

  const handleAddModifier = () => {
    const newModifier: JobModifier = {
      Who: BelongsTo.Victim,
      Rule: JobRule.IfAnyOfThese,
      JobList: []
    };
    onJobModifiersChange([...jobModifiers, newModifier]);
  };

  const handleRemoveModifier = (index: number) => {
    const updatedModifiers = [...jobModifiers];
    updatedModifiers.splice(index, 1);
    onJobModifiersChange(updatedModifiers);
  };

  const handleModifierChange = (index: number, field: keyof JobModifier, value: any) => {
    const updatedModifiers = [...jobModifiers];
    updatedModifiers[index] = {
      ...updatedModifiers[index],
      [field]: value
    };
    onJobModifiersChange(updatedModifiers);
  };

  const handleAddJob = (index: number) => {
    if (!selectedJob) return;
    
    const updatedModifiers = [...jobModifiers];
    if (!updatedModifiers[index].JobList.includes(selectedJob)) {
      updatedModifiers[index] = {
        ...updatedModifiers[index],
        JobList: [...updatedModifiers[index].JobList, selectedJob]
      };
      onJobModifiersChange(updatedModifiers);
    }
    setSelectedJob('');
  };

  const handleRemoveJob = (modifierIndex: number, jobIndex: number) => {
    const updatedModifiers = [...jobModifiers];
    const updatedJobs = [...updatedModifiers[modifierIndex].JobList];
    updatedJobs.splice(jobIndex, 1);
    updatedModifiers[modifierIndex] = {
      ...updatedModifiers[modifierIndex],
      JobList: updatedJobs
    };
    onJobModifiersChange(updatedModifiers);
  };

  return (
    <Box sx={{ mt: 2 }}>
      <FormControlLabel
        control={
          <Switch
            checked={useJobModifiers}
            onChange={(e) => onUseJobModifiersChange(e.target.checked)}
            color="primary"
          />
        }
        label="Use job modifiers"
      />

      {useJobModifiers && (
        <Box sx={{ mt: 2 }}>
          <Button
            variant="outlined"
            startIcon={<AddIcon />}
            onClick={handleAddModifier}
            sx={{ mb: 2 }}
          >
            Add Job Modifier
          </Button>

          {jobModifiers.map((modifier, modifierIndex) => (
            <Paper key={modifierIndex} sx={{ p: 2, mb: 2, position: 'relative' }}>
              <IconButton
                size="small"
                sx={{ position: 'absolute', top: 8, right: 8 }}
                onClick={() => handleRemoveModifier(modifierIndex)}
              >
                <DeleteIcon />
              </IconButton>
              
              <Typography variant="subtitle1" sx={{ mb: 2 }}>
                Job Modifier {modifierIndex + 1}
              </Typography>
              
              <Stack direction="row" spacing={2} sx={{ mb: 2 }}>
                <FormControl fullWidth>
                  <InputLabel id={`who-label-${modifierIndex}`}>Who</InputLabel>
                  <Select
                    labelId={`who-label-${modifierIndex}`}
                    value={modifier.Who}
                    label="Who"
                    onChange={(e) => handleModifierChange(modifierIndex, 'Who', e.target.value)}
                  >
                    <MenuItem value={BelongsTo.Murderer}>Murderer</MenuItem>
                    <MenuItem value={BelongsTo.Victim}>Victim</MenuItem>
                    <MenuItem value={BelongsTo.Player}>Player</MenuItem>
                    <MenuItem value={BelongsTo.MurdererDoctor}>Murderers Doctor</MenuItem>
                    <MenuItem value={BelongsTo.VictimDoctor}>Victims Doctor</MenuItem>
                    <MenuItem value={BelongsTo.MurdererLandlord}>Murderers Landlord</MenuItem>
                    <MenuItem value={BelongsTo.VictimLandlord}>Victims Landlord</MenuItem>
                  </Select>
                </FormControl>
                
                <FormControl fullWidth>
                  <InputLabel id={`rule-label-${modifierIndex}`}>Rule</InputLabel>
                  <Select
                    labelId={`rule-label-${modifierIndex}`}
                    value={modifier.Rule}
                    label="Rule"
                    onChange={(e) => handleModifierChange(modifierIndex, 'Rule', e.target.value)}
                  >
                    <MenuItem value={JobRule.IfAnyOfThese}>If Any Of These</MenuItem>
                    <MenuItem value={JobRule.IfNoneOfThese}>If None Of These</MenuItem>
                  </Select>
                </FormControl>
              </Stack>
              
              <Box sx={{ mt: 2 }}>
                <Typography variant="subtitle2" gutterBottom>
                  Jobs:
                </Typography>
                
                <Stack direction="row" spacing={1} sx={{ flexWrap: 'wrap', gap: 1, mb: 2 }}>
                  {modifier.JobList.map((job, jobIndex) => (
                    <Chip
                      key={jobIndex}
                      label={job}
                      onDelete={() => handleRemoveJob(modifierIndex, jobIndex)}
                      color="primary"
                      variant="outlined"
                    />
                  ))}
                </Stack>
                
                <Stack direction="row" spacing={1}>
                  <Box sx={{ flexGrow: 1 }}>
                    <FormControl fullWidth>
                      <InputLabel id={`job-select-label-${modifierIndex}`}>Add Job</InputLabel>
                      <Select
                        labelId={`job-select-label-${modifierIndex}`}
                        value={selectedJob}
                        label="Add Job"
                        onChange={(e) => setSelectedJob(e.target.value as string)}
                      >
                        {jobList.map((job, index) => (
                          <MenuItem key={index} value={job}>
                            {job}
                          </MenuItem>
                        ))}
                      </Select>
                    </FormControl>
                  </Box>
                  <Button
                    variant="contained"
                    onClick={() => handleAddJob(modifierIndex)}
                    disabled={!selectedJob}
                    sx={{ height: '56px' }}
                  >
                    Add
                  </Button>
                </Stack>
              </Box>
            </Paper>
          ))}
          
          {jobModifiers.length === 0 && (
            <Typography color="text.secondary" sx={{ mt: 1 }}>
              No job modifiers added. Click "Add Job Modifier" to create one.
            </Typography>
          )}
        </Box>
      )}
    </Box>
  );
};

export default JobModifierSelector;
