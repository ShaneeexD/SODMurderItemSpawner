import React, { useState, useEffect } from 'react';
import { 
  Typography, 
  Box, 
  Paper, 
  TextField, 
  List, 
  ListItem, 
  ListItemText,
  Card,
  CardContent,
  Button
} from '@mui/material';
import GitHubIcon from '@mui/icons-material/GitHub';
import SearchIcon from '@mui/icons-material/Search';
import FloorIcon from '@mui/icons-material/Layers';
import floors from '../floors.json';

const Resources: React.FC = () => {
  const [searchTerm, setSearchTerm] = useState('');
  const [filteredFloors, setFilteredFloors] = useState<string[]>([]);
  
  useEffect(() => {
    // Remove duplicates from floors array
    const uniqueFloors = Array.from(new Set(floors as string[]));
    setFilteredFloors(uniqueFloors);
  }, []);

  const handleSearch = (event: React.ChangeEvent<HTMLInputElement>) => {
    const term = event.target.value.toLowerCase();
    setSearchTerm(term);
    
    if (term.trim() === '') {
      // If search is empty, show all unique floors
      const uniqueFloors = Array.from(new Set(floors as string[]));
      setFilteredFloors(uniqueFloors);
    } else {
      // Filter floors based on search term
      const uniqueFloors = Array.from(new Set(floors as string[]));
      const filtered = uniqueFloors.filter(floor => 
        floor.toLowerCase().includes(term)
      );
      setFilteredFloors(filtered);
    }
  };

  const copyToClipboard = (text: string) => {
    navigator.clipboard.writeText(text);
  };

  return (
    <Box sx={{ p: 3, maxWidth: '1200px', margin: '0 auto' }}>
      <Typography variant="h4" component="h1" gutterBottom>
        Resources
      </Typography>
      
      <Box sx={{ display: 'flex', flexDirection: { xs: 'column', md: 'row' }, gap: 4 }}>
        {/* DevTools Section */}
        <Box sx={{ flex: 1 }}>
          <Card elevation={3} sx={{ height: '100%' }}>
            <CardContent>
              <Typography variant="h5" component="h2" gutterBottom>
                <GitHubIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
                DevTools Mod
              </Typography>
              <Typography paragraph>
                The DevTools mod provides additional development tools for SOD (Shadows of Doubt).
                It can help with checking floor names, room presets and furniture presets, as well as a lot more unrelated tools.
                Use commands /room to be shown the current room details based on your location.
                Use commands /furni to be shown the current furniture presets details based on your location, this will help understand what kind of furniture preset is typically found within a room, this will be saved into the mod plugin folder in a JSON list.
              </Typography>
              <Button 
                variant="contained" 
                color="primary" 
                href="https://github.com/ShaneeexD/DevTools-SOD" 
                target="_blank"
                startIcon={<GitHubIcon />}
              >
                View on GitHub
              </Button>
            </CardContent>
          </Card>
        </Box>

        {/* Floor List Section */}
        <Box sx={{ flex: 1 }}>
          <Card elevation={3}>
            <CardContent>
              <Typography variant="h5" component="h2" gutterBottom>
                <FloorIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
                Floor Presets
              </Typography>
              <Typography paragraph>
                Below is a searchable list of all available floor presets that can be used in the game.
                Click on any preset to copy it to clipboard.
              </Typography>
              
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                <SearchIcon sx={{ mr: 1, color: 'text.secondary' }} />
                <TextField
                  fullWidth
                  label="Search Floors"
                  variant="outlined"
                  value={searchTerm}
                  onChange={handleSearch}
                  size="small"
                />
              </Box>
              
              <Paper 
                variant="outlined" 
                sx={{ 
                  maxHeight: '400px', 
                  overflow: 'auto',
                  p: 1
                }}
              >
                <List dense>
                  {filteredFloors.length > 0 ? (
                    filteredFloors.map((floor, index) => (
                      <ListItem 
                        key={index} 
                        onClick={() => copyToClipboard(floor)}
                        sx={{
                          '&:hover': {
                            backgroundColor: 'action.hover',
                            cursor: 'pointer'
                          },
                          borderRadius: '4px',
                          mb: 0.5
                        }}
                      >
                        <ListItemText 
                          primary={floor} 
                          secondary="Click to copy"
                        />
                      </ListItem>
                    ))
                  ) : (
                    <ListItem>
                      <ListItemText primary="No matching floors found" />
                    </ListItem>
                  )}
                </List>
              </Paper>
              <Typography variant="caption" sx={{ display: 'block', mt: 1 }}>
                Total: {filteredFloors.length} floor presets
              </Typography>
            </CardContent>
          </Card>
        </Box>
      </Box>
    </Box>
  );
};

export default Resources;
