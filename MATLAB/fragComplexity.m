close all;
pc_array = cell(1,10);

for i = 1:10
    num = string(i);
    vertFile = load('D:\Dissertation\Dissertation Project VR\pc'+num+'.txt');
    file = 'graph'+num+'.pcd';
    savepcd(file, vertFile);
    pc_array{1,i} = pcread(file);
end

triangles = zeros(1,5);
times = zeros(1,5);

figure; hold on
for i = 1:5
    a = load('D:\Dissertation\Dissertation Project VR\pc'+string((i*2)-1)+'.txt');
    triangles(i) = size(a,2);
    tStart = tic;
    [tran,changed,rms] = pcregrigid(pc_array{1,(i*2)-1},pc_array{1,i},'MaxIterations',50);
    times(i) = toc(tStart);
    
end

xlabel('Number of triangles') % x-axis label
ylabel('Time to match /s') % y-axis label
plot(triangles,times)
