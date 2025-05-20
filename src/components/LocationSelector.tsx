import React from 'react';
import { 
  FormControl, 
  InputLabel, 
  Select, 
  MenuItem, 
  FormHelperText,
  TextField,
  FormControlLabel,
  Switch,
  Paper,
  Typography,
  Stack,
  Box,
  Chip,
  Checkbox,
  ListItemText,
  Button
} from '@mui/material';
import RandomLocationSelector from './RandomLocationSelector';
import type { SelectChangeEvent } from '@mui/material';
import { SpawnLocationType, SubLocationTypeBuildingEntrances } from '../models/configTypes';
import presets from '../presets.json';

interface LocationSelectorProps {
  locationData: {
    locationType: SpawnLocationType;
    // Basic fields
    customRoomName?: string;
    customBuildingPreset?: string;
    buildingEntranceType?: SubLocationTypeBuildingEntrances;
    // Custom position fields
    useCustomPosition?: boolean;
    positionX?: number;
    positionY?: number;
    positionZ?: number;
    // Random location fields
    randomSpawnLocations?: string[];
    // Hotel rooftop bar fields
    hotelRooftopBarSubLocations?: string[];
    // Custom location fields (new list versions)
    customRoomNames?: string[];
    customRoomPresets?: string[];
    customSubRoomNames?: string[];
    customSubRoomPresets?: string[];
    customFloorNames?: string[];
    // Furniture fields
    useFurniture?: boolean;
    furniturePresets?: string[];
    // Mailbox specific
    unlockMailbox?: boolean;
  };
  onChange: (locationData: any) => void;
  unlockMailbox?: boolean;
  setUnlockMailbox?: (unlock: boolean) => void;
}

const LocationSelector: React.FC<LocationSelectorProps> = ({ locationData, onChange, unlockMailbox, setUnlockMailbox }) => {
  // Ensure locationData has default values
  const safeLocationData = {
    ...locationData,
    locationType: locationData.locationType ?? SpawnLocationType.Mailbox,
    randomSpawnLocations: locationData.randomSpawnLocations ?? [],
    hotelRooftopBarSubLocations: locationData.hotelRooftopBarSubLocations ?? [],
    customRoomNames: locationData.customRoomNames ?? [],
    customRoomPresets: locationData.customRoomPresets ?? [],
    customSubRoomNames: locationData.customSubRoomNames ?? [],
    customSubRoomPresets: locationData.customSubRoomPresets ?? [],
    customFloorNames: locationData.customFloorNames ?? []
  };
  
  // Handle location type change
  const handleLocationTypeChange = (event: SelectChangeEvent) => {
    const newLocationType = Number(event.target.value);
    
    // Create a new location data object with only the relevant fields for the selected location type
    const newLocationData: any = {
      locationType: newLocationType,
      // Keep common fields that make sense for all locations
      customRoomName: safeLocationData.customRoomName,
    };
    
    // Only include furniture fields if the location type supports furniture
    if (locationSupportsFurniture(newLocationType)) {
      newLocationData.useFurniture = safeLocationData.useFurniture;
      newLocationData.furniturePresets = safeLocationData.furniturePresets;
    }
    
    // Add location-specific fields based on the new location type
    if (newLocationType === SpawnLocationType.HomeBuildingEntrance || 
        newLocationType === SpawnLocationType.WorkplaceBuildingEntrance) {
      // Building entrance fields
      newLocationData.buildingEntranceType = SubLocationTypeBuildingEntrances.Inside;
    } 
    
    if (newLocationType === SpawnLocationType.Random) {
      // Random location fields
      newLocationData.randomSpawnLocations = [];
    }
    
    if (newLocationType === SpawnLocationType.HotelRooftopBar) {
      // Hotel rooftop bar fields
      newLocationData.hotelRooftopBarSubLocations = [];
    }
    
    if (newLocationType === SpawnLocationType.Custom) {
      // Custom location fields
      newLocationData.customBuildingPreset = '';
      newLocationData.customRoomNames = [];
      newLocationData.customRoomPresets = [];
      newLocationData.customSubRoomNames = [];
      newLocationData.customSubRoomPresets = [];
      newLocationData.customFloorNames = [];
      newLocationData.useCustomPosition = false;
      newLocationData.positionX = 0;
      newLocationData.positionY = 0;
      newLocationData.positionZ = 0;
    }
    
    if (newLocationType === SpawnLocationType.Mailbox) {
      // Mailbox fields
      newLocationData.unlockMailbox = false;
    }
    
    if (newLocationType === SpawnLocationType.Home || 
        newLocationType === SpawnLocationType.Workplace) {
      // Home/Workplace fields
      newLocationData.roomId = '';
    }
    
    onChange(newLocationData);
  };

  // Handle text field changes
  const handleTextChange = (field: string) => (event: React.ChangeEvent<HTMLInputElement>) => {
    onChange({
      ...safeLocationData,
      [field]: event.target.value
    });
  };
  
  // Handle adding an item to a string array
  const handleAddToArray = (field: keyof typeof safeLocationData, value: string) => {
    if (!value.trim()) return; // Don't add empty strings
    
    const currentArray = (safeLocationData[field] as string[]) || [];
    if (!currentArray.includes(value)) {
      onChange({
        ...safeLocationData,
        [field]: [...currentArray, value]
      });
    }
  };
  
  // Handle removing an item from a string array
  const handleRemoveFromArray = (field: keyof typeof safeLocationData, value: string) => {
    const currentArray = (safeLocationData[field] as string[]) || [];
    onChange({
      ...safeLocationData,
      [field]: currentArray.filter((item: string) => item !== value)
    });
  };

  // Handle checkbox changes
  const handleCheckboxChange = (field: string) => (event: React.ChangeEvent<HTMLInputElement>) => {
    onChange({
      ...safeLocationData,
      [field]: event.target.checked
    });
  };

  // Handle building entrance type change
  const handleBuildingEntranceTypeChange = (event: SelectChangeEvent) => {
    onChange({
      ...safeLocationData,
      buildingEntranceType: Number(event.target.value)
    });
  };

  // Position handling removed as custom position feature is no longer needed

  // Handle array field changes (for multi-select)
  const handleArrayChange = (field: string) => (event: any) => {
    const value = event.target.value;
    onChange({
      ...safeLocationData,
      [field]: typeof value === 'string' ? value.split(',') : value
    });
  };

  // Handle removing an item from an array field
  const handleRemoveArrayItem = (field: string, itemToRemove: string) => () => {
    const currentArray = safeLocationData[field as keyof typeof safeLocationData] as string[] || [];
    const newArray = currentArray.filter(item => item !== itemToRemove);
    onChange({
      ...safeLocationData,
      [field]: newArray
    });
  };

  // Determine which fields to show based on location type
  const showBuildingEntranceFields = 
    safeLocationData.locationType === SpawnLocationType.HomeBuildingEntrance || 
    safeLocationData.locationType === SpawnLocationType.WorkplaceBuildingEntrance;
  
  const showRandomLocationFields = safeLocationData.locationType === SpawnLocationType.Random;
  const showHotelRooftopBarFields = safeLocationData.locationType === SpawnLocationType.HotelRooftopBar;
  
  const showCustomRoomFields = safeLocationData.locationType === SpawnLocationType.Custom;
  
  const showFurnitureFields = 
    safeLocationData.locationType === SpawnLocationType.Home || 
    safeLocationData.locationType === SpawnLocationType.Workplace || 
    safeLocationData.locationType === SpawnLocationType.Custom;
  
  const showRoomNameField = 
    safeLocationData.locationType === SpawnLocationType.Home || 
    safeLocationData.locationType === SpawnLocationType.Workplace;
    
  const showMailboxOptions = safeLocationData.locationType === SpawnLocationType.Mailbox;
  
  // Helper function to determine if a location type supports furniture
  const locationSupportsFurniture = (locationType: number) => {
    return locationType === Number(SpawnLocationType.Home) || 
           locationType === Number(SpawnLocationType.Workplace) || 
           locationType === Number(SpawnLocationType.Custom);
  };

  return (
    <Stack spacing={2}>
      <Box>
        <FormControl fullWidth>
          <InputLabel id="spawn-location-label">Spawn Location</InputLabel>
          <Select
            labelId="spawn-location-label"
            id="spawn-location"
            value={safeLocationData.locationType.toString()}
            label="Spawn Location"
            onChange={handleLocationTypeChange}
          >
            <MenuItem value={SpawnLocationType.Home.toString()}>Home</MenuItem>
            <MenuItem value={SpawnLocationType.Workplace.toString()}>Workplace</MenuItem>
            <MenuItem value={SpawnLocationType.HomeLobby.toString()}>Home Lobby</MenuItem>
            <MenuItem value={SpawnLocationType.WorkplaceLobby.toString()}>Workplace Lobby</MenuItem>
            <MenuItem value={SpawnLocationType.HomeBuildingEntrance.toString()}>Home Building Entrance</MenuItem>
            <MenuItem value={SpawnLocationType.WorkplaceBuildingEntrance.toString()}>Workplace Building Entrance</MenuItem>
            <MenuItem value={SpawnLocationType.Mailbox.toString()}>Mailbox</MenuItem>
            <MenuItem value={SpawnLocationType.Doormat.toString()}>Doormat</MenuItem>
            <MenuItem value={SpawnLocationType.CityHallBathroom.toString()}>City Hall Bathroom</MenuItem>
            <MenuItem value={SpawnLocationType.HotelRooftopBar.toString()}>Hotel Rooftop Bar</MenuItem>
            <MenuItem value={SpawnLocationType.Random.toString()}>Random</MenuItem>
            <MenuItem value={SpawnLocationType.Custom.toString()}>Custom Room</MenuItem>
          </Select>
          <FormHelperText>Select where the item should spawn</FormHelperText>
        </FormControl>
      </Box>

      {/* Building Entrance Type (for Home/Workplace Building Entrance) */}
      {showBuildingEntranceFields && (
        <Box>
          <FormControl fullWidth>
            <InputLabel id="building-entrance-type-label">Building Entrance Type</InputLabel>
            <Select
              labelId="building-entrance-type-label"
              id="building-entrance-type"
              value={safeLocationData.buildingEntranceType?.toString() || SubLocationTypeBuildingEntrances.Inside.toString()}
              label="Building Entrance Type"
              onChange={handleBuildingEntranceTypeChange}
            >
              <MenuItem value={SubLocationTypeBuildingEntrances.Inside.toString()}>Inside</MenuItem>
              <MenuItem value={SubLocationTypeBuildingEntrances.Outside.toString()}>Outside</MenuItem>
            </Select>
            <FormHelperText>Select if the item should spawn inside or outside the building entrance</FormHelperText>
          </FormControl>
        </Box>
      )}

      {/* Room Name Field (for Home/Workplace) */}
      {showRoomNameField && (
        <Box>
          <FormControl fullWidth>
            <TextField
              label="Room Name"
              value={safeLocationData.customRoomName || ''}
              onChange={handleTextChange('customRoomName')}
              helperText="Optional: Specify a particular room name"
              variant="outlined"
            />
          </FormControl>
        </Box>
      )}

      {/* Random Location Fields */}
      {showRandomLocationFields && (
        <RandomLocationSelector
          randomLocations={safeLocationData.randomSpawnLocations || []}
          onChange={(locations) => {
            onChange({
              ...safeLocationData,
              randomSpawnLocations: locations
            });
          }}
        />
      )}

      {/* Hotel Rooftop Bar Fields */}
      {showHotelRooftopBarFields && (
        <Box>
          <Paper elevation={2} sx={{ p: 2, mb: 2 }}>
            <Typography variant="subtitle1" gutterBottom>Hotel Rooftop Bar Options</Typography>
            <FormControl fullWidth sx={{ mt: 1 }}>
              <InputLabel id="hotel-sublocations-label">Sub-locations</InputLabel>
              <Select
                labelId="hotel-sublocations-label"
                id="hotel-sublocations"
                multiple
                value={safeLocationData.hotelRooftopBarSubLocations || []}
                onChange={handleArrayChange('hotelRooftopBarSubLocations')}
                renderValue={(selected) => (
                  <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                    {(selected as string[]).map((value) => (
                      <Chip 
                        key={value} 
                        label={value} 
                      />
                    ))}
                  </Box>
                )}
              >
                <MenuItem value="BathroomUnisexEatery">Bathroom</MenuItem>
                <MenuItem value="EateryKitchen">Kitchen</MenuItem>
                <MenuItem value="RooftopBar">Rooftop Bar</MenuItem>
                <MenuItem value="BusinessBackroom">Backroom</MenuItem>
              </Select>
              <FormHelperText>Select specific areas within the hotel rooftop bar</FormHelperText>
            </FormControl>
            
            {/* Selected sub-locations display with delete functionality */}
            {(safeLocationData.hotelRooftopBarSubLocations || []).length > 0 && (
              <Box sx={{ mt: 2 }}>
                <Typography variant="subtitle2" gutterBottom>Selected Sub-locations:</Typography>
                <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                  {(safeLocationData.hotelRooftopBarSubLocations || []).map((value) => (
                    <Chip 
                      key={value} 
                      label={value} 
                      onDelete={handleRemoveArrayItem('hotelRooftopBarSubLocations', value)}
                      sx={{ m: 0.5 }}
                    />
                  ))}
                </Box>
              </Box>
            )}
          </Paper>
        </Box>
      )}

      {/* Custom Location Fields */}
      {showCustomRoomFields && (
        <Box>
          <Paper elevation={2} sx={{ p: 2, mb: 2 }}>
            <Typography variant="subtitle1" gutterBottom>Custom Location Settings</Typography>
            <Stack spacing={2}>
              {/* Building Preset */}
              {/* Floor Names */}
              <Box sx={{ mb: 2 }}>
                <Typography variant="subtitle2" gutterBottom>Floor Names</Typography>
                <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                  <TextField
                    label="Add Floor Name"
                    placeholder="Enter floor name"
                    variant="outlined"
                    size="small"
                    fullWidth
                    onKeyDown={(e: React.KeyboardEvent<HTMLInputElement>) => {
                      if (e.key === 'Enter') {
                        handleAddToArray('customFloorNames', (e.target as HTMLInputElement).value);
                        (e.target as HTMLInputElement).value = '';
                      }
                    }}
                  />
                  <Button 
                    variant="contained" 
                    sx={{ ml: 1 }}
                    onClick={(e: React.MouseEvent<HTMLButtonElement>) => {
                      const input = e.currentTarget.previousElementSibling?.querySelector('input');
                      if (input) {
                        handleAddToArray('customFloorNames', input.value);
                        input.value = '';
                      }
                    }}
                  >
                    Add
                  </Button>
                </Box>
                <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                  {(safeLocationData.customFloorNames || []).map((value) => (
                    <Chip 
                      key={value} 
                      label={value} 
                      onDelete={() => handleRemoveFromArray('customFloorNames', value)} 
                    />
                  ))}
                </Box>
                <FormHelperText>Add custom floor names (e.g., CityHall_GroundFloor, OFA_FirstFloor1)</FormHelperText>
              </Box>
              
              {/* Room Names */}
              <Box sx={{ mb: 2 }}>
                <Typography variant="subtitle2" gutterBottom>Room Names</Typography>
                <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                  <TextField
                    label="Add Room Name"
                    placeholder="Enter room name"
                    variant="outlined"
                    size="small"
                    fullWidth
                    onKeyDown={(e: React.KeyboardEvent<HTMLInputElement>) => {
                      if (e.key === 'Enter') {
                        handleAddToArray('customRoomNames', (e.target as HTMLInputElement).value);
                        (e.target as HTMLInputElement).value = '';
                      }
                    }}
                  />
                  <Button 
                    variant="contained" 
                    sx={{ ml: 1 }}
                    onClick={(e: React.MouseEvent<HTMLButtonElement>) => {
                      const input = e.currentTarget.previousElementSibling?.querySelector('input');
                      if (input) {
                        handleAddToArray('customRoomNames', input.value);
                        input.value = '';
                      }
                    }}
                  >
                    Add
                  </Button>
                </Box>
                <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                  {(safeLocationData.customRoomNames || []).map((value) => (
                    <Chip 
                      key={value} 
                      label={value} 
                      onDelete={() => handleRemoveFromArray('customRoomNames', value)} 
                    />
                  ))}
                </Box>
                <FormHelperText>Add custom room names (e.g., Enforcer Office, Diner, Restaurant)</FormHelperText>
              </Box>
              
              {/* Room Presets */}
              <Box sx={{ mb: 2 }}>
                <Typography variant="subtitle2" gutterBottom>Room Presets</Typography>
                <FormControl fullWidth sx={{ mb: 1 }}>
                  <InputLabel id="room-presets-label">Room Presets</InputLabel>
                  <Select
                    labelId="room-presets-label"
                    id="room-presets"
                    multiple
                    value={safeLocationData.customRoomPresets || []}
                    onChange={handleArrayChange('customRoomPresets')}
                    renderValue={(selected) => (
                      <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                        {(selected as string[]).map((value) => (
                          <Chip key={value} label={value} />
                        ))}
                      </Box>
                    )}
                  >
                    {presets.RoomClassPreset.map((preset) => (
                      <MenuItem key={preset} value={preset}>
                        <Checkbox checked={(safeLocationData.customRoomPresets || []).indexOf(preset) > -1} />
                        <ListItemText primary={preset} />
                      </MenuItem>
                    ))}
                  </Select>
                  <FormHelperText>Select room presets from the list</FormHelperText>
                </FormControl>
                <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                  {(safeLocationData.customRoomPresets || []).map((value) => (
                    <Chip 
                      key={value} 
                      label={value} 
                      onDelete={() => handleRemoveFromArray('customRoomPresets', value)} 
                    />
                  ))}
                </Box>
              </Box>
              
              {/* Sub-Room Names */}
              <Box sx={{ mb: 2 }}>
                <Typography variant="subtitle2" gutterBottom>Sub-Room Names</Typography>
                <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                  <TextField
                    label="Add Sub-Room Name"
                    placeholder="Enter sub-room name"
                    variant="outlined"
                    size="small"
                    fullWidth
                    onKeyDown={(e: React.KeyboardEvent<HTMLInputElement>) => {
                      if (e.key === 'Enter') {
                        handleAddToArray('customSubRoomNames', (e.target as HTMLInputElement).value);
                        (e.target as HTMLInputElement).value = '';
                      }
                    }}
                  />
                  <Button 
                    variant="contained" 
                    sx={{ ml: 1 }}
                    onClick={(e: React.MouseEvent<HTMLButtonElement>) => {
                      const input = e.currentTarget.previousElementSibling?.querySelector('input');
                      if (input) {
                        handleAddToArray('customSubRoomNames', input.value);
                        input.value = '';
                      }
                    }}
                  >
                    Add
                  </Button>
                </Box>
                <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                  {(safeLocationData.customSubRoomNames || []).map((value) => (
                    <Chip 
                      key={value} 
                      label={value} 
                      onDelete={() => handleRemoveFromArray('customSubRoomNames', value)} 
                    />
                  ))}
                </Box>
                <FormHelperText>Add custom sub-room names (e.g., Kitchen, Backroom, Bathroom)</FormHelperText>
              </Box>
              
              {/* Sub-Room Presets */}
              <Box sx={{ mb: 2 }}>
                <Typography variant="subtitle2" gutterBottom>Sub-Room Presets</Typography>
                <FormControl fullWidth sx={{ mb: 1 }}>
                  <InputLabel id="subroom-presets-label">Sub-Room Presets</InputLabel>
                  <Select
                    labelId="subroom-presets-label"
                    id="subroom-presets"
                    multiple
                    value={safeLocationData.customSubRoomPresets || []}
                    onChange={handleArrayChange('customSubRoomPresets')}
                    renderValue={(selected) => (
                      <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                        {(selected as string[]).map((value) => (
                          <Chip key={value} label={value} />
                        ))}
                      </Box>
                    )}
                  >
                    {presets.RoomClassPreset.map((preset) => (
                      <MenuItem key={preset} value={preset}>
                        <Checkbox checked={(safeLocationData.customSubRoomPresets || []).indexOf(preset) > -1} />
                        <ListItemText primary={preset} />
                      </MenuItem>
                    ))}
                  </Select>
                  <FormHelperText>Select sub-room presets from the list</FormHelperText>
                </FormControl>
                <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                  {(safeLocationData.customSubRoomPresets || []).map((value) => (
                    <Chip 
                      key={value} 
                      label={value} 
                      onDelete={() => handleRemoveFromArray('customSubRoomPresets', value)} 
                    />
                  ))}
                </Box>
              </Box>
            </Stack>
          </Paper>
        </Box>
      )}

      {/* Custom Position Fields removed as requested */}

      {/* Mailbox Options */}
      {showMailboxOptions && (
        <Box>
          <Paper elevation={2} sx={{ p: 2, mb: 2 }}>
            <Typography variant="subtitle1" gutterBottom>Mailbox Options</Typography>
            
            <Stack spacing={2}>
              <FormControlLabel
                control={
                  <Switch
                    checked={unlockMailbox || false}
                    onChange={(e) => {
                      if (setUnlockMailbox) {
                        setUnlockMailbox(e.target.checked);
                      }
                    }}
                  />
                }
                label="Unlock Mailbox"
              />
              <FormHelperText>When enabled, the mailbox will be unlocked when the item is spawned</FormHelperText>
            </Stack>
          </Paper>
        </Box>
      )}

      {/* Furniture Fields */}
      {showFurnitureFields && (
        <Box>
          <Paper elevation={2} sx={{ p: 2, mb: 2 }}>
            <Typography variant="subtitle1" gutterBottom>Furniture Settings</Typography>
            
            <Stack spacing={2}>
              <FormControlLabel
                control={
                  <Switch
                    checked={safeLocationData.useFurniture || false}
                    onChange={handleCheckboxChange('useFurniture')}
                  />
                }
                label="Use Furniture for Item Placement"
              />
              
              {safeLocationData.useFurniture && (
                <FormControl fullWidth>
                  <InputLabel id="furniture-presets-label">Furniture Presets</InputLabel>
                  <Select
                    labelId="furniture-presets-label"
                    id="furniture-presets"
                    multiple
                    value={safeLocationData.furniturePresets || []}
                    onChange={handleArrayChange('furniturePresets')}
                    renderValue={(selected) => (
                      <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                        {(selected as string[]).map((value) => (
                          <Chip 
                            key={value} 
                            label={value} 
                            onDelete={handleRemoveArrayItem('furniturePresets', value)}
                            onMouseDown={(event) => {
                              event.stopPropagation();
                            }}
                          />
                        ))}
                      </Box>
                    )}
                  >
                    {/* Add furniture presets from presets.json */}
                    {presets.FurniturePreset.map((preset: string) => (
                      <MenuItem key={preset} value={preset}>
                        <Checkbox checked={(safeLocationData.furniturePresets || []).indexOf(preset) > -1} />
                        <ListItemText primary={preset} />
                      </MenuItem>
                    ))}
                  </Select>
                  <FormHelperText>Select furniture types for item placement</FormHelperText>
                </FormControl>
              )}
            </Stack>
          </Paper>
        </Box>
      )}
    </Stack>
  );
};

export default LocationSelector;
