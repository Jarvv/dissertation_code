% Peforms a point cloud matching proccess using 2 text files. Starts when
% server recieves a response from Unity. Writes the results to 2 text files
% and sends message back to Unity server.
%
% Algoritm usages:
% Matlab 2017b - [tran,changed,rms] = pcregrigid(pc1,pc2,'Verbose',true,'MaxIterations',50,'Extrapolate',true);
% Matlab 2018a - [tran,changed,rms] = pcregistericp(pc2,pc1,'Verbose',true,'MaxIterations',50,'Extrapolate',true);
%              - [tran,changed,rms] = pcregisterndt(pc2,pc1,0.2,'Verbose',true,'MaxIterations',50);

% server
tcpipServer = tcpip('0.0.0.0',55000,'NetworkRole','Server');
while(1)
    fopen(tcpipServer);
    a = load('D:\Dissertation\Dissertation Project VR\pc1.txt');
    b = load('D:\Dissertation\Dissertation Project VR\pc2.txt');
    
    % read the sent message from Unity
    message = fread(tcpipServer,1);
    
    pause(0.5);
    
    savepcd('temp1.pcd',a);
    savepcd('temp2.pcd',b);
    pc1 = pcread('temp1.pcd');
    pc2 = pcread('temp2.pcd');
    
    % Downsample the point clouds using gridAverage
    gridStep = 0.01;
    pc1 = pcdownsample(pc1,'gridAverage',gridStep);
    pc2 = pcdownsample(pc2,'gridAverage',gridStep);
    
    % No vertices selected
    if(message == 49)
        % Point to plane can fail if the inital transformation is not good
        % enough.
        try
            disp('Using point to plane');
            if(size(a,2) > size(b,2))
                  [tran,changed,rms] = pcregistericp (pc2,pc1,'Verbose',true,'Metric','pointToPlane','MaxIterations',50,'Extrapolate',true);
            else
                  [tran,changed,rms] = pcregistericp(pc1,pc2,'Verbose',true,'Metric','pointToPlane','MaxIterations',50,'Extrapolate',true);
            end
        catch
            disp('Using point to point');
            [tran,changed,rms] = pcregistericp(pc1,pc2,'Verbose',true,'MaxIterations',50);
        end
    % Partial Matching using point to point
    elseif(message == 48)
        disp('Partial matching');
        
       if(size(a,2) > size(b,2))
           [tran,changed,rms] = pcregistericp(pc2,pc1,'Verbose',true,'MaxIterations',50,'Extrapolate',true);
       else
           [tran,changed,rms] = pcregistericp(pc1,pc2,'Verbose',true,'MaxIterations',50,'Extrapolate',true);
       end
    end
    fclose(tcpipServer);
    
    % Client
    if(isempty(a))
    else
        tcpipClient = tcpip('127.0.0.1',55001,'NetworkRole','Client');
        set(tcpipClient,'Timeout',30);
        fp = fopen('D:\Dissertation\Dissertation Project VR\result.txt','wt');
        
        % Write the rms and trs matrix to a file
        fprintf(fp,'%f\n',rms);
        for i = 1:4
            for j = 1:4
                fprintf(fp,'%f\n',tran.T(j,i));
            end
        end
        fclose(fp);
        fopen(tcpipClient);
        if(size(a,2) > size(b,2))
            fwrite(tcpipClient,'fragment02');
         else
            fwrite(tcpipClient,'fragment01');
         end
        fclose(tcpipClient);
    end
end