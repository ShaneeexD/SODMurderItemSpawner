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
import { BelongsTo, TraitRule } from '../models/configTypes';
import type { TraitModifier } from '../models/configTypes';
import presetsData from '../presets.json';

interface TraitModifierSelectorProps {
  useTraits: boolean;
  onUseTraitsChange: (value: boolean) => void;
  traitModifiers: TraitModifier[];
  onTraitModifiersChange: (modifiers: TraitModifier[]) => void;
}

const TraitModifierSelector: React.FC<TraitModifierSelectorProps> = ({
  useTraits,
  onUseTraitsChange,
  traitModifiers,
  onTraitModifiersChange
}) => {
  const [selectedTrait, setSelectedTrait] = useState<string>('');

  // Get trait list from presets
  const traitList = presetsData.CharacterTrait || [];

  const handleAddModifier = () => {
    const newModifier: TraitModifier = {
      Who: BelongsTo.Victim,
      Rule: TraitRule.IfAnyOfThese,
      TraitList: []
    };
    onTraitModifiersChange([...traitModifiers, newModifier]);
  };

  const handleRemoveModifier = (index: number) => {
    const updatedModifiers = [...traitModifiers];
    updatedModifiers.splice(index, 1);
    onTraitModifiersChange(updatedModifiers);
  };

  const handleModifierChange = (index: number, field: keyof TraitModifier, value: any) => {
    const updatedModifiers = [...traitModifiers];
    updatedModifiers[index] = {
      ...updatedModifiers[index],
      [field]: value
    };
    onTraitModifiersChange(updatedModifiers);
  };

  const handleAddTrait = (index: number) => {
    if (!selectedTrait) return;
    
    const updatedModifiers = [...traitModifiers];
    if (!updatedModifiers[index].TraitList.includes(selectedTrait)) {
      updatedModifiers[index] = {
        ...updatedModifiers[index],
        TraitList: [...updatedModifiers[index].TraitList, selectedTrait]
      };
      onTraitModifiersChange(updatedModifiers);
    }
    setSelectedTrait('');
  };

  const handleRemoveTrait = (modifierIndex: number, traitIndex: number) => {
    const updatedModifiers = [...traitModifiers];
    const updatedTraits = [...updatedModifiers[modifierIndex].TraitList];
    updatedTraits.splice(traitIndex, 1);
    updatedModifiers[modifierIndex] = {
      ...updatedModifiers[modifierIndex],
      TraitList: updatedTraits
    };
    onTraitModifiersChange(updatedModifiers);
  };

  return (
    <Box sx={{ mt: 2 }}>
      <FormControlLabel
        control={
          <Switch
            checked={useTraits}
            onChange={(e) => onUseTraitsChange(e.target.checked)}
            color="primary"
          />
        }
        label="Use trait modifiers"
      />

      {useTraits && (
        <Box sx={{ mt: 2 }}>
          <Button
            variant="outlined"
            startIcon={<AddIcon />}
            onClick={handleAddModifier}
            sx={{ mb: 2 }}
          >
            Add Trait Modifier
          </Button>

          {traitModifiers.map((modifier, modifierIndex) => (
            <Paper key={modifierIndex} sx={{ p: 2, mb: 2, position: 'relative' }}>
              <IconButton
                size="small"
                sx={{ position: 'absolute', top: 8, right: 8 }}
                onClick={() => handleRemoveModifier(modifierIndex)}
              >
                <DeleteIcon />
              </IconButton>
              
              <Typography variant="subtitle1" sx={{ mb: 2 }}>
                Trait Modifier {modifierIndex + 1}
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
                    <MenuItem value={TraitRule.IfAnyOfThese}>If Any Of These</MenuItem>
                    <MenuItem value={TraitRule.IfAllOfThese}>If All Of These</MenuItem>
                    <MenuItem value={TraitRule.IfNoneOfThese}>If None Of These</MenuItem>
                  </Select>
                </FormControl>
              </Stack>
              
              <Box sx={{ mt: 2 }}>
                <Typography variant="subtitle2" gutterBottom>
                  Traits:
                </Typography>
                
                <Stack direction="row" spacing={1} sx={{ flexWrap: 'wrap', gap: 1, mb: 2 }}>
                  {modifier.TraitList.map((trait, traitIndex) => (
                    <Chip
                      key={traitIndex}
                      label={trait}
                      onDelete={() => handleRemoveTrait(modifierIndex, traitIndex)}
                      color="primary"
                      variant="outlined"
                    />
                  ))}
                </Stack>
                
                <Stack direction="row" spacing={1}>
                  <Box sx={{ flexGrow: 1 }}>
                    <FormControl fullWidth>
                      <InputLabel id={`trait-select-label-${modifierIndex}`}>Add Trait</InputLabel>
                      <Select
                        labelId={`trait-select-label-${modifierIndex}`}
                        value={selectedTrait}
                        label="Add Trait"
                        onChange={(e) => setSelectedTrait(e.target.value as string)}
                      >
                        {traitList.map((trait, index) => (
                          <MenuItem key={index} value={trait}>
                            {trait}
                          </MenuItem>
                        ))}
                      </Select>
                    </FormControl>
                  </Box>
                  <Button
                    variant="contained"
                    onClick={() => handleAddTrait(modifierIndex)}
                    disabled={!selectedTrait}
                    sx={{ height: '56px' }}
                  >
                    Add
                  </Button>
                </Stack>
              </Box>
            </Paper>
          ))}
          
          {traitModifiers.length === 0 && (
            <Typography color="text.secondary" sx={{ mt: 1 }}>
              No trait modifiers added. Click "Add Trait Modifier" to create one.
            </Typography>
          )}
        </Box>
      )}
    </Box>
  );
};

export default TraitModifierSelector;
