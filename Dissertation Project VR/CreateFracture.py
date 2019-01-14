import bpy
import bmesh
from random import *
import mathutils
import numpy as np

# Thickness of the fragments
F_THICKNESS = 0.5

# Lookup tables have to be generated when indexing bm faces or verts
def lookup(bm):
        if hasattr(bm.faces, "ensure_lookup_table"):
                bm.faces.ensure_lookup_table()

        if hasattr(bm.verts, "ensure_lookup_table"):
                bm.verts.ensure_lookup_table()

# Using an array of vertex indicies, colour each one
def colour_vertex(obj, vert_array):
        colour = (0.0,1.0,0.0)

        mesh = obj.data
        bpy.ops.object.mode_set(mode='OBJECT')

        #Check if mesh already has vertex colours, if not create new
        if mesh.vertex_colors:
                vcol_layer = mesh.vertex_colors.active
        else:
                vcol_layer = mesh.vertex_colors.new()

        # Assign the vertex its colour
        for vert in vert_array:
                for poly in mesh.polygons:
                        for loop_index in poly.loop_indices:
                                loop_vert_index = mesh.loops[loop_index].vertex_index
                                if vert == loop_vert_index:
                                        vcol_layer.data[loop_index].color = colour

def init_params():
        # Setting up the parameters through the input of the user
        # Prompts the user to enter the number of cuts for the fragments
        n_cuts = None
        while n_cuts is None:
                try:
                        n_cuts=int(input('Degree of subdivision:'))
                except ValueError:
                        print("Not a number")

        # Prompts the user to enter how much noise they want to give to the vertices of the fragments
        noise = None
        while noise is None or noise > 1.0 or noise < 0 :
                try:
                        noise=float(input('Degree of noise (0.0 = no noise, 1.0 = max noise):'))
                except ValueError:
                        print("Not a number")

        # Prompts the user to enter the fragment complexity (extra subdivisons)
        complexity = None
        while complexity is None:
                try:
                        complexity=int(input('Complexity increase magnitude: '))
                except ValueError:
                        print("Not a number")

        # Number of vertices on the subdivided face
        n_verts = (n_cuts + 2)**2

        # Assign a random number in the range of the thickness of the fragment by how much the
        # position of the vertex should be altered.
        vert_change_array = []
        for x in range(0, n_verts):
                vert_change_array.append(uniform(-F_THICKNESS/2,F_THICKNESS/2))

        # Perform the 'fracturing' with these params
        do_fracture(n_cuts,noise,complexity,n_verts,vert_change_array)

def do_fracture(n_cuts,noise,complexity,n_verts,vert_change_array):
        # Get all objects in the scene
        scene = bpy.context.scene

        # Run twice, for standard fragments and noisy fragments
        for i in range(0,2):
                for obj in scene.objects: 
                        if obj.type == 'MESH':

                                if obj.name == "TopMesh":
                                        f_face = 0
                                        vert_counter = 0
                                if obj.name == "BotMesh":
                                        f_face = 1
                                        vert_counter = 4

                                # Select the current object, put it into edit mode and get it's details
                                scene.objects.active = obj
                                bpy.ops.object.mode_set(mode='EDIT')
                                me = obj.data
                                bm = bmesh.from_edit_mesh(me)

                                # Standard fragments
                                if i == 0:
                                        
                                        lookup(bm)
                                                
                                        # Subdivide the fracture face into a number of cuts.
                                        bmesh.ops.subdivide_edges(bm,
                                                edges=bm.faces[f_face].edges,
                                                use_grid_fill=True,
                                                cuts=n_cuts)
                                        
                                        lookup(bm)
                                        
                                        for x in vert_change_array:
                                                
                                                # Translate the bottom vertices for the top fragment
                                                if obj.name == "TopMesh" and vert_counter == 4:
                                                        vert_counter = 8

                                                # As Blender counts vertices in a different direction for each face, make sure that each translation
                                                # is done on the correct coressponding inner verticies.
                                                if obj.name == "TopMesh" and vert_counter > (len(vert_change_array) - ((n_cuts**2) - 4)):
                                                        
                                                        # Makes the top iterate vertically not hoirontally to match with the bottom
                                                        if vert_counter != ((len(vert_change_array)+3) - (n_cuts-1)) and vert_counter > (len(vert_change_array) - (n_cuts - 4)):
                                                                vert_counter -= ((n_cuts**2) - n_cuts)
                                                        else:
                                                                vert_counter += (n_cuts - 1)
                                                
                                                bmesh.ops.translate(bm,
                                                        vec = mathutils.Vector((0.0,0.0,x)),
                                                        space = obj.matrix_world,
                                                        verts = [bm.verts[vert_counter]])

                                                vert_counter += 1

                                        lookup(bm)
                                        
                                        # Performing increase of complexity, select all faces and subdivide
                                        for f in bm.faces:
                                                f.select = True

                                        for z in range(0,complexity):
                                                bpy.ops.mesh.subdivide()
                                
                                        # Apply all changes to selected object
                                        me.update()

                                        # Leave edit mode so next fragment can be selected
                                        bpy.ops.object.mode_set(mode='OBJECT')
                                        obj.select = False
                        
                                # Noisy fragments
                                if i == 1 and noise != 0.0:
                                        lookup(bm)

                                        vert_sum = 0
                                        paint_vert_array = []
                                        
                                        for v in bm.verts:

                                                # Create a value of noise depending on the z-axis value and the degree
                                                # of noise chosen.
                                                z_noise = 0                                            
                                                if obj.name == "BotMesh":                                     
                                                        if v.co.z > -0.40:
                                                                z_noise = noise * (((((v.co.z*-1) / 0.40)-1)))
                                                                paint_vert_array.append(v.index)
                                                                
                                                        # Creating random fractures
                                                        elif uniform(0,1) > 0.999 and v.co.z > -0.5:                                                       
                                                                z_noise = noise * -0.07                                
                                                                paint_vert_array.append(v.index)

                                                if obj.name == "TopMesh":
                                                        if v.co.z < -1.1                :
                                                                z_noise = noise * (((((v.co.z*-1) / 1.1)-1)))
                                                                paint_vert_array.append(v.index)
                                                                
                                                        # Creating random fractures
                                                        elif uniform(0,1) > 0.999 and v.co.z < -1:                                                       
                                                                z_noise = noise * 0.07                                
                                                                paint_vert_array.append(v.index)

                                                # 'Breaking' off tips of the fragment
                                                bmesh.ops.translate(bm,
                                                        vec = mathutils.Vector((0.0,0.0, z_noise)),
                                                        space = obj.matrix_world,
                                                        verts = [v])
                                                        
                                                vert_sum += (z_noise)**2
                                        
                                        if obj.name == "BotMesh":                                     
                                                obj.name = "BotMeshNoise" + str(n_cuts)

                                        colour_vertex(obj,paint_vert_array)

                                        # Apply to selected object
                                        me.update()

                                        # Leave edit mode so next fragment can be selected
                                        bpy.ops.object.mode_set(mode='OBJECT')
                
                if i == 0:
                        # Save the file into the Unity projects Assets folder where it can be loaded straight into the scene.
                        bpy.ops.wm.save_as_mainfile(filepath="D:\\Dissertation\\Dissertation Project VR\\Assets\\Meshes\\fractured" + str(n_cuts) + ".blend")
                
                if i == 1:
                        # Save the file into the Unity projects Assets folder where it can be loaded straight into the scene.
                        bpy.ops.wm.save_as_mainfile(filepath="D:\\Dissertation\\Dissertation Project VR\\Assets\\Meshes\\fractured" + str(n_cuts) + "noise.blend")
                        
                        # Calculate the rmse for the noisy fragment, save to file to be read by unity.
                        rmse = np.sqrt(vert_sum/len(vert_change_array))
                        with open("D:\\Dissertation\\Dissertation Project VR\\fractured" + str(n_cuts) + "rmse.txt", 'w') as file:
                                file.write(str(rmse))

        # Go into paint mode to see noise changes
        bpy.ops.object.mode_set(mode='VERTEX_PAINT')

# Start
init_params()